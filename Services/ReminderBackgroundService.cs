using FinansSitesi.Data;
using Microsoft.EntityFrameworkCore;

namespace FinansSitesi.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ReminderBackgroundService> _logger;

        public ReminderBackgroundService(IServiceProvider services, ILogger<ReminderBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await CheckAndSendReminders(dbContext, emailService);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Her 5 dakikada bir kontrol
            }
        }

        private async Task CheckAndSendReminders(ApplicationDbContext dbContext, IEmailService emailService)
        {
            var now = DateTime.UtcNow;
            var reminders = await dbContext.Reminders
                .Include(r => r.User)
                .Where(r => !r.IsCompleted &&
                           !r.IsEmailSent &&
                           (r.ReminderDate <= now.AddDays(1) && r.ReminderDate > now.AddDays(1).AddMinutes(-10) || // 1 gün kala
                           r.ReminderDate <= now.AddHours(3) && r.ReminderDate > now.AddHours(3).AddMinutes(-10)))  // 3 saat kala
                .ToListAsync();

            foreach (var reminder in reminders)
            {
                try

                {
                    var timeLeft = reminder.ReminderDate - now;
                    var timeText = timeLeft.TotalHours >= 24 ? "1 gün" : "3 saat";

                    var subject = $"Hatırlatıcı: {reminder.Title} ({timeText} kala)";
                    var message = $@"
                    <h2>{reminder.Title}</h2>
                    <p>{reminder.Description}</p>
                    <p><strong>Tarih:</strong> {reminder.ReminderDate.ToString("f")}</p>
                    <p>Bu hatırlatıcı {timeText} sonra gerçekleşecek.</p>";

                    await emailService.SendReminderEmailAsync(reminder);

                    reminder.IsEmailSent = true;
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Hatırlatıcı e-postası gönderildi: {reminder.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Hatırlatıcı e-postası gönderilirken hata: {reminder.Id}");
                }
            }
        }
    }
}

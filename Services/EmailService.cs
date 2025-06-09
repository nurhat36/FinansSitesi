using System.Net.Mail;
using System.Net;
using FinansSitesi.Models;

namespace FinansSitesi.Services
{
    public interface IEmailService
    {
        Task SendReminderEmailAsync(Reminder reminder);

    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendReminderEmailAsync(Reminder reminder)
        {
            var timeLeft = reminder.ReminderDate - DateTime.UtcNow;
            var timeText = timeLeft.TotalHours >= 24 ? "1 gün" : "3 saat";

            var subject = $"Hatırlatıcı: {reminder.Title} ({timeText} kala)";

            // Razor template render
            var viewPath = Path.Combine("Templates", "ReminderEmail.cshtml");
            var template = await System.IO.File.ReadAllTextAsync(viewPath);
            var htmlContent = template.Replace("@Model.Title", reminder.Title)
                                     .Replace("@Model.Description", reminder.Description)
                                     .Replace("@Model.ReminderDate", reminder.ReminderDate.ToString("f"));

            await SendEmailAsync(reminder.User.Email, subject, htmlContent);
        }

        

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient
            {
                Host = _config["EmailSettings:SmtpServer"],
                Port = int.Parse(_config["EmailSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:FromEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}

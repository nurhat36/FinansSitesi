using FinansSitesi.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using FinansSitesi.Models;
using System.Linq;

namespace FinansSitesi.Services
{
    public class RecurringTransactionService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RecurringTransactionService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var today = DateTime.Now.Date;

                var recurringTransactions = await context.RecurringTransactions
                    .Where(r => !r.EndDate.HasValue || r.EndDate.Value.Date >= today)
                    .ToListAsync(stoppingToken);

                foreach (var r in recurringTransactions)
                {
                    // Başlangıç tarihi: LastGeneratedDate varsa onun tarihi, yoksa StartDate
                    DateTime lastDate = r.LastGeneratedDate?.Date ?? r.StartDate.Date;

                    // Sonraki tarih hesaplama, lastDate'ın tarih kısmı önemli
                    DateTime nextDate = GetNextDate(lastDate, r.Frequency).Date;

                    Console.WriteLine($"LastGeneratedDate: {r.LastGeneratedDate}, NextDate: {nextDate}, Frequency: {r.Frequency}");

                    // nextDate bugünün tarihi veya öncesindeyse işlem yap, bitiş tarihini aşmamalı
                    while (nextDate <= today && (!r.EndDate.HasValue || nextDate <= r.EndDate.Value.Date))
                    {
                        // İşlem zaten varsa atla
                        bool exists = await context.Transactions.AnyAsync(t =>
                            t.AccountId == r.AccountId &&
                            // Saat olmadan karşılaştır
                            t.Description.ToLower() == (r.Description ?? "").ToLower() && // Küçük harf karşılaştırması
                            t.Amount == r.Amount,
                            stoppingToken);

                        Console.WriteLine($"Checking for existing transaction on {nextDate.ToShortDateString()}: Exists = {exists}");

                        if (!exists)
                        {
                            var transaction = new Transaction
                            {
                                AccountId = r.AccountId,
                                Amount = r.Amount,
                                CategoryId = r.CategoryId,
                                Date = nextDate,
                                Description = r.Description,
                                Type = r.Type,
                                UserId = r.UserId,
                                IsRecurring = true
                            };
                            var account = await context.Accounts.FindAsync(r.AccountId);
                            if (account != null)
                            {
                                // 🔁 Trigger mantığını uyguluyoruz
                                if (r.Type == "Income")
                                {
                                    account.Balance += r.Amount;
                                }
                                else if (r.Type == "Expense")
                                {
                                    account.Balance -= r.Amount;
                                }
                            }

                            context.Transactions.Add(transaction);
                            await context.SaveChangesAsync(stoppingToken);

                            Console.WriteLine($"Added transaction for {nextDate.ToShortDateString()}");
                            
                        }
                        break;

                        // LastGeneratedDate güncelle
                        r.LastGeneratedDate = nextDate;
                        await context.SaveChangesAsync(stoppingToken);

                        // Sonraki tarihi al
                        nextDate = GetNextDate(nextDate, r.Frequency).Date;
                    }
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private DateTime GetNextDate(DateTime current, string frequency)
        {
            if (frequency == null)
                throw new ArgumentNullException(nameof(frequency));

            switch (frequency.ToLowerInvariant())
            {
                case "daily":
                    return current.AddDays(1);
                case "weekly":
                    return current.AddDays(7);
                case "monthly":
                    return current.AddMonths(1);
                default:
                    throw new ArgumentException($"Invalid frequency value: {frequency}");
            }
        }
    }
}

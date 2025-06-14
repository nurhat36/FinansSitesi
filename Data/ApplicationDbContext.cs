﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinansSitesi.Models;

namespace FinansSitesi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<GoalTransaction> GoalTransactions { get; set; }
        // Data/ApplicationDbContext.cs
        public DbSet<Reminder> Reminders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>()
       .Property(a => a.Balance)
       .HasPrecision(18, 2);

            modelBuilder.Entity<Budget>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinancialGoal>()
                .Property(f => f.CurrentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinancialGoal>()
                .Property(f => f.TargetAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RecurringTransaction>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            // Transaction -> User ilişkisi (tek UserId var)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Cascade kaldırıldı
            modelBuilder.Entity<Transaction>()
    .HasOne(t => t.User)
    .WithMany(u => u.Transactions)  // Eğer ApplicationUser'da Transactions varsa
    .HasForeignKey(t => t.UserId);


            // Transaction -> Account ilişkisi
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RecurringTransaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.RecurringTransactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction -> Category ilişkisi
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

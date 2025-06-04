using FinansSitesi.Models;
using Microsoft.AspNetCore.Identity;
using System;
namespace FinansSitesi.Models { 
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public string ProfileImageUrl { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Account> Accounts { get; set; }
        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<FinancialGoal> FinancialGoals { get; set; }
        public ICollection<Note> Notes { get; set; }

    }
}
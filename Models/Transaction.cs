using System.ComponentModel.DataAnnotations.Schema;

namespace FinansSitesi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public decimal Amount { get; set; }
        public string Type { get; set; } // Income, Expense, Transfer

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }

        public string? Description { get; set; }
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; }
    }
}

namespace FinansSitesi.Models
{
    // RecurringTransaction.cs
    public class RecurringTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public decimal Amount { get; set; }
        public string Type { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string Frequency { get; set; } // Daily, Weekly, Monthly
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
    }
}

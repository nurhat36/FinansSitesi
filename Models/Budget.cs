namespace FinansSitesi.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public decimal Amount { get; set; }
        public string Period { get; set; } // Monthly, Weekly
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}

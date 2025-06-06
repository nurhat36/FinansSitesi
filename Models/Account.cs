namespace FinansSitesi.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<RecurringTransaction> RecurringTransactions { get; set; }
    }
}

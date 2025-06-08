namespace FinansSitesi.Models.ViewModels
{
    public class AccountBalanceViewModel
    {
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public decimal OriginalBalance { get; set; }
        public decimal BalanceInTRY { get; set; }
        public decimal BalanceInUSD { get; set; }
        public decimal BalanceInEUR { get; set; }
    }
}

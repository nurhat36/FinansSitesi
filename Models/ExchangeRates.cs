namespace FinansSitesi.Models
{
    public class ExchangeRates
    {
        public string Base { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
        public DateTime Date { get; set; }
    }

}

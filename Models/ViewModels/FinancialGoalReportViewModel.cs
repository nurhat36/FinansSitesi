namespace FinansSitesi.Models.ViewModels
{
    public class FinancialGoalReportViewModel
    {
        public FinancialGoal Goal { get; set; }
        public List<GoalTransaction> Transactions { get; set; }
        public decimal TotalAdded { get; set; }
        public decimal RemainingAmount { get; set; }
        public int DaysRemaining { get; set; }
        public decimal DailyNeeded { get; set; }

        // Grafik verileri için
        public List<string> ChartLabels { get; set; }
        public List<decimal> ChartData { get; set; }
    }
}

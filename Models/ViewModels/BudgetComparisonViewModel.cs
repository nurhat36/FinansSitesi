namespace FinansSitesi.Models.ViewModels
{
    public class BudgetComparisonViewModel
    {
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public string Period { get; set; }
    }
}

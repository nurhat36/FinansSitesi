namespace FinansSitesi.Models.ViewModels
{
    public class BudgetComparisonViewModel
    {
        public string CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal Remaining => BudgetAmount - SpentAmount;
        public double SpentPercentage => BudgetAmount == 0 ? 0 : (double)(SpentAmount / BudgetAmount) * 100;
    }

}

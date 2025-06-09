using FinansSitesi.Models;

namespace FinansSitesi.ViewModels
{
    public class FinancialGoalViewModel
    {
        public FinancialGoal Goal { get; set; }
        public double CompletionRate
        {
            get
            {
                return Goal.TargetAmount == 0 ? 0 : Math.Min(100, (double)(Goal.CurrentAmount / Goal.TargetAmount) * 100);
            }
        }
    }
}

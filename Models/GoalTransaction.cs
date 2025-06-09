using System.ComponentModel.DataAnnotations;

namespace FinansSitesi.Models
{
    public class GoalTransaction
    {
        public int Id { get; set; }
        public int FinancialGoalId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation property
        public FinancialGoal? FinancialGoal { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace FinansSitesi.Models.ViewModels
{
    public class AddAmountViewModel
    {
        public int FinancialGoalId { get; set; }

        [Display(Name = "Mevcut Tutar")]
        public decimal CurrentAmount { get; set; }

        [Required(ErrorMessage = "Lütfen bir tutar giriniz.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır.")]
        [Display(Name = "Eklenecek Tutar")]
        public decimal AmountToAdd { get; set; }

        [Display(Name = "Açıklama (Opsiyonel)")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string? Description { get; set; }

        [Display(Name = "İşlem Tarihi")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;
    }
}
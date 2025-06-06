using System.ComponentModel.DataAnnotations;
namespace FinansSitesi.Models.ViewModels
{
    public class AccountCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Currency { get; set; }
    }

}

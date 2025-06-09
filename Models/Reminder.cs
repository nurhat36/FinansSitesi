using System.ComponentModel.DataAnnotations;

namespace FinansSitesi.Models
{
    // Models/Reminder.cs
    public class Reminder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime ReminderDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsEmailSent { get; set; } = false;
    }
}

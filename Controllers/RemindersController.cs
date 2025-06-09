using FinansSitesi.Data;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinansSitesi.Controllers
{
    // Controllers/RemindersController.cs
    [Authorize]
    public class RemindersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RemindersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reminders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var reminders = await _context.Reminders
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.ReminderDate)
                .ToListAsync();

            return View(reminders);
        }

        // POST: Reminders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ReminderDate")] Reminder reminder)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                reminder.UserId = user.Id;
                reminder.IsCompleted = false;

                _context.Add(reminder);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // POST: Reminders/Complete/5
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder != null)
            {
                reminder.IsCompleted = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // POST: Reminders/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder != null)
            {
                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}

using FinansSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinansSitesi.Data;

namespace FinansSitesi.Controllers
{
    [Authorize]
    public class RecurringTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RecurringTransactions/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View();
        }

        // POST: RecurringTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecurringTransaction recurringTransaction)
        {
            if (!ModelState.IsValid)
            {
                recurringTransaction.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.RecurringTransactions.Add(recurringTransaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Transactions");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(recurringTransaction);
        }
    }
}

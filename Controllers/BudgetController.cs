using FinansSitesi.Data;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinansSitesi.Controllers
{
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Giriş yapmış kullanıcı Id'si
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(budgets);
        }
        // GET: Budget/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                return NotFound();

            // Kategorileri dropdown için ViewData'ya yükle
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            ViewBag.Periods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Aylık", Text = "Aylık" },
        new SelectListItem { Value = "Haftalık", Text = "Haftalık" },
        new SelectListItem { Value = "Yıllık", Text = "Yıllık" }
    };

            return View(budget);
        }

        // POST: Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Budgets.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(budget);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            ViewBag.Periods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Aylık", Text = "Aylık" },
        new SelectListItem { Value = "Haftalık", Text = "Haftalık" },
        new SelectListItem { Value = "Yıllık", Text = "Yıllık" }
    };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Budget budget)
        {
            if (!ModelState.IsValid)
            {
                // Otomatik olarak giriş yapan kullanıcının ID'sini al
                budget.UserId = _userManager.GetUserId(User);

                _context.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", budget.CategoryId);
            return View(budget);
        }

    }
}

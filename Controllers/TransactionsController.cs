using FinansSitesi.Data;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinansSitesi.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Giriş yapmış kullanıcının Id'sini alıyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserId: {userId}"); // Debug için UserId'yi konsola yazdırıyoruz

            if (string.IsNullOrEmpty(userId))
            {
                // Kullanıcı giriş yapmamışsa veya userId bulunamazsa, uygun bir yönlendirme yapabilirsiniz
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının işlemlerini çekiyoruz, ilişkili Category ve Account ile birlikte
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }


        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View();
        }
        // GET: Transactions/CreateRecurring
        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                transaction.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                transaction.Date = DateTime.Now;

                // İlgili hesabı veritabanından çek
                var account = await _context.Accounts.FindAsync(transaction.AccountId);
                if (account == null)
                {
                    ModelState.AddModelError("AccountId", "Seçilen hesap bulunamadı.");
                    ViewBag.Categories = _context.Categories.ToList();
                    ViewBag.Accounts = _context.Accounts.ToList();
                    return View(transaction);
                }

                if (transaction.Type == "Income")
                {
                    account.Balance += transaction.Amount;
                }
                else if (transaction.Type == "Expense")
                {
                    if (account.Balance < transaction.Amount)
                    {
                        ModelState.AddModelError("Amount", "Bakiyeniz bu gideri karşılayacak kadar yüksek değil.");
                        ViewBag.Categories = _context.Categories.ToList();
                        ViewBag.Accounts = _context.Accounts.ToList();
                        return View(transaction);
                    }
                    account.Balance -= transaction.Amount;
                }
                else
                {
                    ModelState.AddModelError("Type", "Geçersiz işlem türü.");
                    ViewBag.Categories = _context.Categories.ToList();
                    ViewBag.Accounts = _context.Accounts.ToList();
                    return View(transaction);
                }

                // İşlem kaydet
                _context.Transactions.Add(transaction);

                // Hesap güncelle
                _context.Accounts.Update(account);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View(transaction);
        }



        [HttpGet]
        public IActionResult GetCategoriesByType(string type)
        {
            if (string.IsNullOrEmpty(type))
                return Json(new List<object>()); // boş liste döndür

            var categories = _context.Categories
                .Where(c => c.Type == type)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    icon = c.Icon
                })
                .ToList();

            return Json(categories);
        }

    }
}

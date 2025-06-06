using FinansSitesi.Data;
using FinansSitesi.Models.ViewModels;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinansSitesi.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Yeni hesap oluşturma sayfası
        public IActionResult CreateAccount()
        {
            return View();
        }

        // POST: Yeni hesap oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAccount(AccountCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Giriş yapan kullanıcının ID'sini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Yeni hesap nesnesini oluştur
            var account = new Account
            {
                UserId = userId,
                Name = model.Name,
                Currency = model.Currency,
                Balance = 0m // Hesap açılışında bakiye 0
            };

            // Veritabanına ekle
            _context.Accounts.Add(account);
            _context.SaveChanges();

            // Hesap listesine yönlendir
            return RedirectToAction("Index");
        }

        // Kullanıcının hesaplarını listele
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var accounts = _context.Accounts.Where(a => a.UserId == userId).ToList();
            return View(accounts);
        }
    }
}

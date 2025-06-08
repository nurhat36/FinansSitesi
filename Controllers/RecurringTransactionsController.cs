using FinansSitesi.Data;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        // GET: RecurringTransactions
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = await _context.RecurringTransactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Account)  // Hesap bilgisi için
                .Include(t => t.Category) // Kategori bilgisi için
                .ToListAsync();
            return View(transactions);
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
                // Kullanıcı ID’si atanıyor
                recurringTransaction.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // İlgili hesabı getiriyoruz
                var account = await _context.Accounts.FindAsync(recurringTransaction.AccountId);
                if (account != null)
                {
                    // Trigger mantığını uyguluyoruz
                    if (recurringTransaction.Type == "Income")
                    {
                        account.Balance += recurringTransaction.Amount;
                    }
                    else if (recurringTransaction.Type == "Expense")
                    {
                        account.Balance -= recurringTransaction.Amount;
                    }
                }

                _context.RecurringTransactions.Add(recurringTransaction);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View(recurringTransaction);
        }

        // GET: RecurringTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var transaction = await _context.RecurringTransactions.FindAsync(id);
            if (transaction == null)
                return NotFound();
            Console.WriteLine(transaction.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Sadece kendi kullanıcısının işlemini düzenleyebilir
            if (transaction.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View(transaction);
        }

        // POST: RecurringTransactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecurringTransaction recurringTransaction)
        {
            if (id != recurringTransaction.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    // Önce eski işlemi bul
                    var existingTransaction = await _context.RecurringTransactions.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTransaction != null)
                    {
                        // Eski hesabın bakiyesini geri al
                        var oldAccount = await _context.Accounts.FindAsync(existingTransaction.AccountId);
                        if (oldAccount != null)
                        {
                            if (existingTransaction.Type == "Income")
                                oldAccount.Balance -= existingTransaction.Amount;
                            else if (existingTransaction.Type == "Expense")
                                oldAccount.Balance += existingTransaction.Amount;
                        }
                    }

                    // Yeni hesabın bakiyesini güncelle
                    var newAccount = await _context.Accounts.FindAsync(recurringTransaction.AccountId);
                    if (newAccount != null)
                    {
                        if (recurringTransaction.Type == "Income")
                            newAccount.Balance += recurringTransaction.Amount;
                        else if (recurringTransaction.Type == "Expense")
                            newAccount.Balance -= recurringTransaction.Amount;
                    }

                    // Giriş yapan kullanıcının ID’sini ata
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    recurringTransaction.UserId = userId;

                    _context.Update(recurringTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecurringTransactionExists(recurringTransaction.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // ViewBag'leri tekrar gönder
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Accounts = _context.Accounts.ToList();
            return View(recurringTransaction);
        }


        // GET: RecurringTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var transaction = await _context.RecurringTransactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (transaction == null)
                return NotFound();

            // Sadece kendi kullanıcısının işlemini silebilir
            if (transaction.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(transaction);
        }

        // POST: RecurringTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.RecurringTransactions.FindAsync(id);
            if (transaction != null)
            {
                // Sadece kendi kullanıcısının işlemini silebilir
                if (transaction.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                    return Forbid();

                // Hesap bakiyesini güncelle
                var account = await _context.Accounts.FindAsync(transaction.AccountId);
                if (account != null)
                {
                    if (transaction.Type == "Income")
                        account.Balance -= transaction.Amount;
                    else if (transaction.Type == "Expense")
                        account.Balance += transaction.Amount;
                }

                _context.RecurringTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RecurringTransactionExists(int id)
        {
            return _context.RecurringTransactions.Any(e => e.Id == id);
        }
    }
}

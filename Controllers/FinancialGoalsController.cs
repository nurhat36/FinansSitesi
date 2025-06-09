using FinansSitesi.Data;
using FinansSitesi.Models;
using FinansSitesi.Models.ViewModels;
using FinansSitesi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Rotativa.AspNetCore.Options;
using Rotativa.AspNetCore;

namespace FinansSitesi.Controllers
{
    public class FinancialGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinancialGoalsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var goals = await _context.FinancialGoals
                .Where(g => g.UserId == user.Id)
                .ToListAsync();

            var goalViewModels = goals.Select(g => new FinancialGoalViewModel
            {
                Goal = g
            }).ToList();

            return View(goalViewModels);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinancialGoal goal)
        {
            var user = await _userManager.GetUserAsync(User);
            goal.UserId = user.Id;
            goal.CurrentAmount = 0;

            if (!ModelState.IsValid)
            {
                _context.Add(goal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(goal);
        }
        // GET: FinancialGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goal = await _context.FinancialGoals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // POST: FinancialGoals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,TargetAmount,CurrentAmount,DueDate")] FinancialGoal updatedGoal)
        {
            if (id != updatedGoal.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingGoal = await _context.FinancialGoals.FindAsync(id);
                    if (existingGoal == null)
                    {
                        return NotFound();
                    }

                    // Sadece güncellenebilir alanları değiştiriyoruz
                    existingGoal.Title = updatedGoal.Title;
                    existingGoal.Description = updatedGoal.Description;
                    existingGoal.TargetAmount = updatedGoal.TargetAmount;
                    existingGoal.CurrentAmount = updatedGoal.CurrentAmount;
                    existingGoal.DueDate = updatedGoal.DueDate;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinancialGoalExists(updatedGoal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(updatedGoal);
        }

        private bool FinancialGoalExists(int id)
        {
            return _context.FinancialGoals.Any(e => e.Id == id);
        }


        // GET: FinancialGoals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var goal = await _context.FinancialGoals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            _context.FinancialGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: FinancialGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(m => m.Id == id);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }
        [HttpGet]
        public IActionResult AddAmount(int id)
        {
            var goal = _context.FinancialGoals.Find(id);
            if (goal == null)
            {
                return NotFound();
            }

            var viewModel = new AddAmountViewModel
            {
                FinancialGoalId = id,
                CurrentAmount = goal.CurrentAmount,
                TransactionDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAmount(AddAmountViewModel model)
        {
            Console.WriteLine("AddAmount method called with model: " + model.FinancialGoalId);
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid.");
                var goal = _context.FinancialGoals.Find(model.FinancialGoalId);
                Console.WriteLine($"FinancialGoalId: {model.FinancialGoalId}, CurrentAmount: {goal?.CurrentAmount}");
                if (goal == null)
                {
                    return NotFound();
                }

                // Tutarı güncelle
                goal.CurrentAmount += model.AmountToAdd;

                // İşlem geçmişi oluştur (opsiyonel)
                var transaction = new GoalTransaction
                {
                    
                    FinancialGoalId = model.FinancialGoalId,
                    Amount = model.AmountToAdd,
                    Description = model.Description,
                    TransactionDate = model.TransactionDate,
                    CreatedDate = DateTime.Now
                };

                _context.GoalTransactions.Add(transaction);
                _context.SaveChanges();

                return RedirectToAction("Details", new { id = model.FinancialGoalId });
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult GenerateReport(int id)
        {
            var goal = _context.FinancialGoals
                .Include(g => g.Transactions)
                .FirstOrDefault(g => g.Id == id);

            if (goal == null) return NotFound();

            var transactions = goal.Transactions?.OrderBy(t => t.TransactionDate).ToList() ?? new List<GoalTransaction>();

            var totalAdded = transactions.Sum(t => t.Amount);
            var remaining = Math.Max(goal.TargetAmount - goal.CurrentAmount, 0);
            var daysRemaining = Math.Max((goal.DueDate - DateTime.Today).Days, 0);
            var dailyNeeded = daysRemaining > 0 ? remaining / daysRemaining : 0;

            var chartData = new List<decimal>();
            var chartLabels = new List<string>();
            var runningTotal = 0m;

            foreach (var transaction in transactions)
            {
                runningTotal += transaction.Amount;
                chartData.Add(runningTotal);
                chartLabels.Add(transaction.TransactionDate.ToString("dd MMM"));
            }

            return View("Report", new FinancialGoalReportViewModel
            {
                Goal = goal,
                Transactions = transactions,
                TotalAdded = totalAdded,
                RemainingAmount = remaining,
                DaysRemaining = daysRemaining,
                DailyNeeded = dailyNeeded,
                ChartLabels = chartLabels,
                ChartData = chartData
            });
        }

        public IActionResult DownloadPdfReport(int id)
        {
            var goal = _context.FinancialGoals
                .Include(g => g.Transactions)
                .FirstOrDefault(g => g.Id == id);

            if (goal == null) return NotFound();

            var transactions = goal.Transactions?.OrderBy(t => t.TransactionDate).ToList() ?? new List<GoalTransaction>();

            var totalAdded = transactions.Sum(t => t.Amount);
            var remaining = Math.Max(goal.TargetAmount - goal.CurrentAmount, 0);
            var daysRemaining = Math.Max((goal.DueDate - DateTime.Today).Days, 0);
            var dailyNeeded = daysRemaining > 0 ? remaining / daysRemaining : 0;

            var chartData = new List<decimal>();
            var chartLabels = new List<string>();
            var runningTotal = 0m;

            foreach (var transaction in transactions)
            {
                runningTotal += transaction.Amount;
                chartData.Add(runningTotal);
                chartLabels.Add(transaction.TransactionDate.ToString("dd MMM"));
            }

            return new ViewAsPdf("Report", new FinancialGoalReportViewModel
            {
                Goal = goal,
                Transactions = transactions,
                TotalAdded = totalAdded,
                RemainingAmount = remaining,
                DaysRemaining = daysRemaining,
                DailyNeeded = dailyNeeded,
                ChartLabels = chartLabels,
                ChartData = chartData
            })
            {
                FileName = $"{goal.Title}_Rapor_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                PageMargins = { Left = 10, Right = 10, Top = 10, Bottom = 10 }
            };
        }



        // İsteğe bağlı: Edit, Delete, Details vs.
    }

}

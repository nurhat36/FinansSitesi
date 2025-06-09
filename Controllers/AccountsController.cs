using FinansSitesi.Data;
using FinansSitesi.Models.ViewModels;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace FinansSitesi.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context; // varsayalım db context

        public AccountsController(IMemoryCache memoryCache, HttpClient httpClient, ApplicationDbContext context)
        {
            _memoryCache = memoryCache;
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<IActionResult> HesapBakiyeleri()
        {
            var cacheKey = "ExchangeRatesCache";
            if (!_memoryCache.TryGetValue(cacheKey, out ExchangeRates exchangeRates))
            {
                // Cache’de yok, API’den al
                var apiKey = "Ia00ngsIN8VCLW5JYaKoTItigXwKXUa7";
                var url = "https://api.apilayer.com/exchangerates_data/latest?base=TRY";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", apiKey);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return View("Error");

                var json = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonSerializer.Deserialize<ExchangeRates>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Cache’e ekle, 10 dakika boyunca sakla
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10)); // 10 dakika boyunca kullanılmazsa silinir
                _memoryCache.Set(cacheKey, exchangeRates, cacheEntryOptions);
            }

            // Artık exchangeRates verisi cache’den veya API’den hazır

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var usdRate = exchangeRates.Rates["USD"];
            var eurRate = exchangeRates.Rates["EUR"];

            var accountBalances = accounts.Select(a =>
            {
                decimal balanceInTry = a.Currency switch
                {
                    "TRY" => a.Balance,
                    "USD" => a.Balance / usdRate,
                    "EUR" => a.Balance / eurRate,
                    _ => a.Balance
                };

                return new AccountBalanceViewModel
                {
                    AccountName = a.Name,
                    Currency = a.Currency,
                    OriginalBalance = a.Balance,
                    BalanceInTRY = Math.Round(balanceInTry, 2),
                    BalanceInUSD = Math.Round(balanceInTry * usdRate, 2),
                    BalanceInEUR = Math.Round(balanceInTry * eurRate, 2)
                };
            }).ToList();

            // Para birimine göre toplamlar (TRY, USD, EUR ayrı ayrı)
            var totalByCurrency = accountBalances
     .GroupBy(ab => ab.Currency)
     .Select(g => new
     {
         Currency = g.Key,
         Total = g.Sum(x => x.OriginalBalance)
     })
     .ToList();

            ViewData["TotalByCurrency"] = totalByCurrency;


            // TRY cinsinden toplam bakiye (tüm hesapların TRY cinsine dönüştürülmüş hali)
            decimal totalBalanceInTry = accountBalances.Sum(ab => ab.BalanceInTRY);

            // ViewModel içine toplam bilgileri de ekleyebilirsin veya ViewData ile gönderebilirsin
            
            ViewData["TotalBalanceInTRY"] = totalBalanceInTry;


            return View(accountBalances);
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

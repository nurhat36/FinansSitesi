using FinansSitesi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FinansSitesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task<IActionResult> GuncelKur()
        {
            var apiKey = "Ia00ngsIN8VCLW5JYaKoTItigXwKXUa7"; // Kendi anahtarýný yaz
            var url = $"https://api.apilayer.com/exchangerates_data/latest?base=USD";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("apikey", apiKey);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return View("Error");

            var json = await response.Content.ReadAsStringAsync();

            var exchangeRates = JsonSerializer.Deserialize<ExchangeRates>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (exchangeRates == null || !exchangeRates.Rates.ContainsKey("TRY"))
                return View("Error");

            // TRY bazlý oranlarý hesapla
            var popularCurrencies = new List<string> { "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "CNY", "TRY" };

            // TRY bazlý oranlarý hesapla
            var tryRate = exchangeRates.Rates["TRY"];
            var ratesInTry = new Dictionary<string, decimal>();

            foreach (var rate in exchangeRates.Rates)
            {
                var rateToTry = tryRate / rate.Value;
                ratesInTry[rate.Key] = Math.Round(rateToTry, 4);
            }

            // Popülerleri en baþa al, geri kalanlarý sona
            var sortedRates = ratesInTry
                .OrderByDescending(r => popularCurrencies.Contains(r.Key)) // popülerler önce
                .ThenBy(r => r.Key) // alfabetik sýraya göre
                .ToDictionary(r => r.Key, r => r.Value);

            var model = new ExchangeRates
            {
                Base = "TRY",
                Date = exchangeRates.Date,
                Rates = sortedRates
            };


            return View(model);
        }



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

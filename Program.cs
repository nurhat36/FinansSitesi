using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinansSitesi.Data; // ApplicationDbContext burada tanımlı
using FinansSitesi.Models; // ApplicationUser burada tanımlı
using ArackiralamaProje.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using FinansSitesi.Services;
using Rotativa.AspNetCore;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// 2. Identity hizmeti (kullanıcı ve rol desteği)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Özel servisler ve e-posta servisi
// Servisleri ekleyin
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

// appsettings.json'a e-posta ayarları

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHostedService<RecurringTransactionService>();

// 4. MVC, Razor ve diğer servisler

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

var app = builder.Build();

// 📄 ROTATIVA AYARI – wwwroot/Rotativa/wkhtmltopdf.exe yolunu kullanır
RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// 5. Hata yönetimi ve güvenlik
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. Kimlik doğrulama ve yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

// 7. Rotalar
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// 8. Uygulama başlat
app.Run();

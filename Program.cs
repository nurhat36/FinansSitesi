using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinansSitesi.Data; // ApplicationDbContext burada tanımlı
using FinansSitesi.Models; // ApplicationUser burada tanımlı
using ArackiralamaProje.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using FinansSitesi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IEmailSender, EmailSender>();
// 2. Identity hizmetini ekle (rol desteği ile)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()

.AddDefaultTokenProviders();

// 3. MVC ve Razor Pages
builder.Services.AddHostedService<RecurringTransactionService>();
// .NET 6+ için Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Identity Razor Pages (Login, Register, vs.) için gerekli

var app = builder.Build();

// 4. Hata yönetimi ve güvenlik ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. Kimlik doğrulama ve yetkilendirme middleware
app.UseAuthentication();
app.UseAuthorization();

// 6. Rotalar
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages (Login, Register, vs.)
app.MapRazorPages();

app.Run();

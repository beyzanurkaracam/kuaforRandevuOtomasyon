using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using kuaforBerberOtomasyon.Models;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantý dizesini al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext'i kaydet
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(connectionString));

// Kimlik doðrulama hizmetlerini ekleyin
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login";  // Giriþ sayfasý
                    options.LogoutPath = "/Home/Logout";  // Çýkýþ sayfasý
                });

// Controller'larý ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP isteklerini iþleme sýrasýnda kimlik doðrulama ve yetkilendirme ara yazýlýmlarýný ekleyin
app.UseRouting();           // Routing middleware

app.UseAuthentication();    // Kimlik doðrulama middleware
app.UseAuthorization();     // Yetkilendirme middleware

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();  // HTTP isteklerini HTTPS'e yönlendir
app.UseStaticFiles();       // Statik dosyalarý (CSS, JS, resimler) sunmak için

// Varsayýlan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

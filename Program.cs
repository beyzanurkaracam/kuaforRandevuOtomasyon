using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using kuaforBerberOtomasyon.Models;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant� dizesini al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext'i kaydet
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(connectionString));

// Kimlik do�rulama hizmetlerini ekleyin
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login";  // Giri� sayfas�
                    options.LogoutPath = "/Home/Logout";  // ��k�� sayfas�
                });

// Controller'lar� ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP isteklerini i�leme s�ras�nda kimlik do�rulama ve yetkilendirme ara yaz�l�mlar�n� ekleyin
app.UseRouting();           // Routing middleware

app.UseAuthentication();    // Kimlik do�rulama middleware
app.UseAuthorization();     // Yetkilendirme middleware

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection();  // HTTP isteklerini HTTPS'e y�nlendir
app.UseStaticFiles();       // Statik dosyalar� (CSS, JS, resimler) sunmak i�in

// Varsay�lan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

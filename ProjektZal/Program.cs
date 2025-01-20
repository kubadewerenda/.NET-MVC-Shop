using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjektZal.Models;

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Konfiguracja DbContext z SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie kontroler�w z widokami
builder.Services.AddControllersWithViews();

// Konfiguracja uwierzytelniania przy u�yciu Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // U�ywamy domy�lnego schematu "Cookies"
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
    {
        config.LoginPath = "/User/Login"; // �cie�ka do logowania
        config.AccessDeniedPath = "/User/AccessDenied"; // Opcjonalnie: �cie�ka dla odmowy dost�pu
    });

var app = builder.Build();

// Obs�uga b��d�w w trybie produkcyjnym
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Uwierzytelnianie
app.UseAuthorization(); // Autoryzacja

app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;
    await next();
});

// Konfiguracja domy�lnej trasy
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uruchomienie aplikacji
app.Run();

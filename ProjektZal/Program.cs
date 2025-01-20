using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjektZal.Models;

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) 
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, config =>
    {
        config.LoginPath = "/User/Login";
        config.AccessDeniedPath = "/User/AccessDenied"; 
    });

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization(); 
app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = cultureInfo;
    CultureInfo.CurrentUICulture = cultureInfo;
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

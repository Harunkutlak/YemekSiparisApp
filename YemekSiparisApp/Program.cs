using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using YemekSiparisApp.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC servislerini ekle
builder.Services.AddControllersWithViews();

// Entity Framework Core - SQL Server bağlantısı
builder.Services.AddDbContext<YemekSiparisContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("YemekSiparisDB")));

// Cookie tabanlı kimlik doğrulama
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Hesap/Giris";
        options.LogoutPath = "/Hesap/Cikis";
        options.AccessDeniedPath = "/Hesap/ErisimEngellendi";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "YemekSiparisAuth";
    });

// Session desteği
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Hata işleme
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Hata");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Rota yapılandırması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YemekSiparisApp.Data;
using YemekSiparisApp.Models.Entities;
using YemekSiparisApp.Models.ViewModels;

namespace YemekSiparisApp.Controllers
{
    public class HesapController : Controller
    {
        private readonly YemekSiparisContext _context;

        public HesapController(YemekSiparisContext context)
        {
            _context = context;
        }

        // GET: /Hesap/Giris
        public IActionResult Giris()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Hesap/Giris
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var sifreHash = HashSifre(model.Sifre);
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email == model.Email
                                       && k.SifreHash == sifreHash
                                       && k.IsActive);

            if (kullanici == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }

            await SignInAsync(kullanici);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Hesap/Kayit
        public IActionResult Kayit() => View();

        // POST: /Hesap/Kayit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Email ve telefon benzersizlik kontrolü
            if (await _context.Kullanicilar.AnyAsync(k => k.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(model);
            }
            if (await _context.Kullanicilar.AnyAsync(k => k.Telefon == model.Telefon))
            {
                ModelState.AddModelError("Telefon", "Bu telefon numarası zaten kayıtlı.");
                return View(model);
            }

            var kullanici = new Kullanici
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                Telefon = model.Telefon,
                SifreHash = HashSifre(model.Sifre),
                Adres = model.Adres,
                Rol = "Musteri"
            };

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();

            await SignInAsync(kullanici);
            return RedirectToAction("Index", "Home");
        }

        // GET/POST: /Hesap/Cikis
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ErisimEngellendi() => View();

        // ── Yardımcı Metodlar ──────────────────────────────────────────────

        private static string HashSifre(string sifre)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sifre));
            return Convert.ToBase64String(bytes);
        }

        private async Task SignInAsync(Kullanici kullanici)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
                new Claim(ClaimTypes.Name, $"{kullanici.Ad} {kullanici.Soyad}"),
                new Claim(ClaimTypes.Email, kullanici.Email),
                new Claim(ClaimTypes.Role, kullanici.Rol),
                new Claim("IsIhtiyacSahibi", kullanici.IsIhtiyacSahibi.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

            if (kullanici.Rol == "Kurye")
                return RedirectToAction("Index", "Kurye");

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

        // ══════════════════════════════════════════════════════════════════════
        //  PROFİL GÜNCELLEME
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Hesap/Profil
        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciId == userId);

            if (kullanici == null) return RedirectToAction(nameof(Cikis));

            // Aktif başvuruyu yükle (en son)
            var basvuru = await _context.AskidaYemekBasvurulari
                .Where(b => b.KullaniciId == userId)
                .OrderByDescending(b => b.BasvuruTarihi)
                .FirstOrDefaultAsync();

            ViewBag.Basvuru = basvuru;

            var model = new ProfilViewModel
            {
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Telefon = kullanici.Telefon,
                Adres = kullanici.Adres,
                Email = kullanici.Email,
                Rol = kullanici.Rol,
                OlusturulmaTarihi = kullanici.OlusturulmaTarihi,
            };

            return View(model);
        }

        // POST: /Hesap/Basvur
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Basvur(string? basvuruNotu)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Beklemedeki veya onaylanmış başvuru varsa tekrar başvurulamaz
            var mevcutBasvuru = await _context.AskidaYemekBasvurulari
                .Where(b => b.KullaniciId == userId && b.Durum != "Reddedildi")
                .FirstOrDefaultAsync();

            if (mevcutBasvuru != null)
            {
                TempData["Hata"] = "Zaten aktif bir başvurunuz bulunuyor.";
                return RedirectToAction(nameof(Profil));
            }

            var basvuru = new AskidaYemekBasvuru
            {
                KullaniciId = userId,
                Durum = "Beklemede",
                BasvuruNotu = basvuruNotu,
                BasvuruTarihi = DateTime.Now
            };

            _context.AskidaYemekBasvurulari.Add(basvuru);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Başvurunuz alındı! Admin inceledikten sonra sonucu profilinizde görebilirsiniz.";
            return RedirectToAction(nameof(Profil));
        }

        // POST: /Hesap/BasvuruIptal
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BasvuruIptal()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var basvuru = await _context.AskidaYemekBasvurulari
                .Where(b => b.KullaniciId == userId && b.Durum == "Beklemede")
                .FirstOrDefaultAsync();

            if (basvuru != null)
            {
                _context.AskidaYemekBasvurulari.Remove(basvuru);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Başvurunuz iptal edildi.";
            }

            return RedirectToAction(nameof(Profil));
        }


        // POST: /Hesap/ProfilGuncelle
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilGuncelle(ProfilViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var kullanici = await _context.Kullanicilar
                .Include(k => k.Siparisler)
                .FirstOrDefaultAsync(k => k.KullaniciId == userId);

            if (kullanici == null) return RedirectToAction(nameof(Cikis));

            // Şifre değişikliği isteniyorsa valide et
            bool sifreDegisecek = !string.IsNullOrWhiteSpace(model.YeniSifre);
            if (sifreDegisecek)
            {
                if (string.IsNullOrWhiteSpace(model.MevcutSifre))
                {
                    ModelState.AddModelError("MevcutSifre", "Şifre değiştirmek için mevcut şifrenizi giriniz.");
                }
                else if (kullanici.SifreHash != HashSifre(model.MevcutSifre))
                {
                    ModelState.AddModelError("MevcutSifre", "Mevcut şifre hatalı.");
                }
            }

            // Telefon benzersizlik kontrolü (kendi numarası hariç)
            if (await _context.Kullanicilar.AnyAsync(k => k.Telefon == model.Telefon && k.KullaniciId != userId))
            {
                ModelState.AddModelError("Telefon", "Bu telefon numarası başka bir hesapta kullanılıyor.");
            }

            // ViewModel readonly alanlarını geri doldur (ModelState için)
            model.Email = kullanici.Email;
            model.Rol = kullanici.Rol;
            model.OlusturulmaTarihi = kullanici.OlusturulmaTarihi;
            model.ToplamSiparis = kullanici.Siparisler.Count;

            // Sadece düzenlenebilir alanları valide et
            ModelState.Remove(nameof(ProfilViewModel.Email));
            ModelState.Remove(nameof(ProfilViewModel.Rol));
            ModelState.Remove(nameof(ProfilViewModel.OlusturulmaTarihi));
            ModelState.Remove(nameof(ProfilViewModel.ToplamSiparis));
            ModelState.Remove(nameof(ProfilViewModel.MevcutSifre));
            ModelState.Remove(nameof(ProfilViewModel.YeniSifreTekrar));

            if (!ModelState.IsValid)
            {
                TempData["Hata"] = "Lütfen formdaki hataları düzeltin.";
                return View("Profil", model);
            }

            // Güncelle
            kullanici.Ad = model.Ad;
            kullanici.Soyad = model.Soyad;
            kullanici.Telefon = model.Telefon;
            kullanici.Adres = model.Adres;
            kullanici.GuncellenmeTarihi = DateTime.Now;

            if (sifreDegisecek && !string.IsNullOrWhiteSpace(model.YeniSifre))
            {
                kullanici.SifreHash = HashSifre(model.YeniSifre);
            }

            await _context.SaveChangesAsync();

            // Cookie'yi yenile (navbar'da isim güncellensin)
            await SignInAsync(kullanici);

            TempData["Mesaj"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profil));
        }

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

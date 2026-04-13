using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using YemekSiparisApp.Data;
using YemekSiparisApp.Models.Entities;

namespace YemekSiparisApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly YemekSiparisContext _context;

        public AdminController(YemekSiparisContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DASHBOARD
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.ToplamKullanici = await _context.Kullanicilar.CountAsync();
            ViewBag.ToplamSiparis = await _context.Siparisler.CountAsync();
            ViewBag.ToplamRestoran = await _context.Restoranlar.CountAsync();
            ViewBag.ToplamHavuzBakiye = await _context.AskidaYemekHavuzlari.SumAsync(h => h.ToplamBakiye);

            return View();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  RESTORAN CRUD
        // ══════════════════════════════════════════════════════════════════════

        // READ - Tüm restoranları listele
        // GET: /Admin/Restoranlar
        public async Task<IActionResult> Restoranlar()
        {
            var restoranlar = await _context.Restoranlar
                .Include(r => r.SahipKullanici)
                .OrderByDescending(r => r.RestoranId)
                .ToListAsync();

            ViewBag.Sahipler = await _context.Kullanicilar
                .Where(k => k.Rol == "RestoranSahibi" && k.SahipOlduguRestoran == null)
                .ToListAsync();

            // Düzenleme için tüm RestoranSahibi kullanıcıları
            ViewBag.TumSahipler = await _context.Kullanicilar
                .Where(k => k.Rol == "RestoranSahibi")
                .ToListAsync();

            return View(restoranlar);
        }

        // CREATE - Yeni restoran ekle
        [HttpPost]
        public async Task<IActionResult> RestoranEkle(Restoran model)
        {
            // Navigation property'leri validation'dan çıkar
            ModelState.Remove("SahipKullanici");

            if (ModelState.IsValid)
            {
                model.IsActive = true;
                model.ToplamCiro = 0;
                model.PuanSayisi = 0;
                model.OlusturulmaTarihi = DateTime.Now;

                _context.Restoranlar.Add(model);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Restoran başarıyla eklendi.";
            }
            else
            {
                TempData["Hata"] = "Lütfen bilgileri geçerli formatta giriniz.";
            }

            return RedirectToAction(nameof(Restoranlar));
        }

        // UPDATE - Restoran bilgilerini güncelle
        [HttpPost]
        public async Task<IActionResult> RestoranGuncelle(int restoranId, string ad, string? aciklama, 
            string adres, string? telefon, string? email, string? sehir,
            int sahipKullaniciId, int tahminiTeslimatDakika, int minSiparisUcreti)
        {
            var restoran = await _context.Restoranlar.FindAsync(restoranId);
            if (restoran != null)
            {
                restoran.Ad = ad;
                restoran.Aciklama = aciklama;
                restoran.Adres = adres;
                restoran.Telefon = telefon;
                restoran.Email = email;
                restoran.Sehir = sehir;
                restoran.SahipKullaniciId = sahipKullaniciId;
                restoran.TahminiTeslimatDakika = tahminiTeslimatDakika;
                restoran.MinSiparisUcreti = minSiparisUcreti;

                await _context.SaveChangesAsync();
                TempData["Mesaj"] = $"\"{ad}\" restoranı başarıyla güncellendi.";
            }
            else
            {
                TempData["Hata"] = "Restoran bulunamadı.";
            }

            return RedirectToAction(nameof(Restoranlar));
        }

        // UPDATE - Restoran aktif/pasif durum değiştir
        [HttpPost]
        public async Task<IActionResult> RestoranDurumDegistir(int id)
        {
            var restoran = await _context.Restoranlar.FindAsync(id);
            if (restoran != null)
            {
                restoran.IsActive = !restoran.IsActive;
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = restoran.IsActive 
                    ? $"\"{restoran.Ad}\" restoranı aktif edildi." 
                    : $"\"{restoran.Ad}\" restoranı pasif edildi.";
            }
            return RedirectToAction(nameof(Restoranlar));
        }

        // DELETE - Restoran sil (kalıcı silme)
        [HttpPost]
        public async Task<IActionResult> RestoranSil(int id)
        {
            var restoran = await _context.Restoranlar
                .Include(r => r.MenuKalemleri)
                .Include(r => r.Siparisler)
                .FirstOrDefaultAsync(r => r.RestoranId == id);

            if (restoran == null)
            {
                TempData["Hata"] = "Restoran bulunamadı.";
                return RedirectToAction(nameof(Restoranlar));
            }

            // Aktif siparişi olan restoran silinemez
            if (restoran.Siparisler.Any(s => s.Durum != "TeslimEdildi" && s.Durum != "Iptal"))
            {
                TempData["Hata"] = "Aktif siparişleri olan restoran silinemez. Önce tüm siparişleri kapatın.";
                return RedirectToAction(nameof(Restoranlar));
            }

            // Menü kalemlerini soft-delete et
            foreach (var menu in restoran.MenuKalemleri)
            {
                menu.IsActive = false;
            }

            // Restoranı soft-delete et (IsActive = false)
            restoran.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = $"\"{restoran.Ad}\" restoranı başarıyla silindi (soft-delete).";

            return RedirectToAction(nameof(Restoranlar));
        }

        // READ - Restoran detay (JSON - AJAX için)
        [HttpGet]
        public async Task<IActionResult> RestoranDetay(int id)
        {
            var restoran = await _context.Restoranlar
                .Include(r => r.SahipKullanici)
                .FirstOrDefaultAsync(r => r.RestoranId == id);

            if (restoran == null) return NotFound();

            return Json(new
            {
                restoran.RestoranId,
                restoran.Ad,
                restoran.Aciklama,
                restoran.Adres,
                restoran.Telefon,
                restoran.Email,
                restoran.Sehir,
                restoran.SahipKullaniciId,
                SahipAdi = restoran.SahipKullanici?.Ad + " " + restoran.SahipKullanici?.Soyad,
                restoran.Puan,
                restoran.PuanSayisi,
                restoran.ToplamCiro,
                restoran.MinSiparisUcreti,
                restoran.TahminiTeslimatDakika,
                restoran.IsActive,
                OlusturulmaTarihi = restoran.OlusturulmaTarihi.ToString("dd.MM.yyyy HH:mm")
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  KULLANICI CRUD
        // ══════════════════════════════════════════════════════════════════════

        // READ - Tüm kullanıcıları listele
        // GET: /Admin/Kullanicilar
        public async Task<IActionResult> Kullanicilar()
        {
            var kullanicilar = await _context.Kullanicilar
                .OrderByDescending(k => k.KullaniciId)
                .ToListAsync();
            return View(kullanicilar);
        }

        // CREATE - Yeni kullanıcı ekle
        [HttpPost]
        public async Task<IActionResult> KullaniciEkle(string ad, string soyad, string email, 
            string telefon, string sifre, string rol, string? adres, bool isIhtiyacSahibi = false)
        {
            // Email benzersizlik kontrolü
            if (await _context.Kullanicilar.AnyAsync(k => k.Email == email))
            {
                TempData["Hata"] = "Bu e-posta adresi zaten kayıtlı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Telefon benzersizlik kontrolü
            if (await _context.Kullanicilar.AnyAsync(k => k.Telefon == telefon))
            {
                TempData["Hata"] = "Bu telefon numarası zaten kayıtlı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            var kullanici = new Kullanici
            {
                Ad = ad,
                Soyad = soyad,
                Email = email,
                Telefon = telefon,
                SifreHash = HashSifre(sifre),
                Rol = rol,
                Adres = adres,
                IsIhtiyacSahibi = isIhtiyacSahibi,
                IsActive = true,
                OlusturulmaTarihi = DateTime.Now
            };

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = $"\"{ad} {soyad}\" kullanıcısı başarıyla oluşturuldu.";

            return RedirectToAction(nameof(Kullanicilar));
        }

        // UPDATE - Kullanıcı bilgilerini güncelle
        [HttpPost]
        public async Task<IActionResult> KullaniciGuncelle(int id, string ad, string soyad, 
            string email, string telefon, string rol, string? adres, 
            bool isIhtiyacSahibi = false, string? yeniSifre = null)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Admin kendi rolünü bozmasın
            if (kullanici.KullaniciId == 1 && rol != "Admin")
            {
                TempData["Hata"] = "Ana admin hesabının rolü değiştirilemez.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Email benzersizlik kontrolü (kendi emaili hariç)
            if (await _context.Kullanicilar.AnyAsync(k => k.Email == email && k.KullaniciId != id))
            {
                TempData["Hata"] = "Bu e-posta adresi başka bir kullanıcıda kayıtlı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Telefon benzersizlik kontrolü (kendi telefonu hariç)
            if (await _context.Kullanicilar.AnyAsync(k => k.Telefon == telefon && k.KullaniciId != id))
            {
                TempData["Hata"] = "Bu telefon numarası başka bir kullanıcıda kayıtlı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            kullanici.Ad = ad;
            kullanici.Soyad = soyad;
            kullanici.Email = email;
            kullanici.Telefon = telefon;
            kullanici.Rol = rol;
            kullanici.Adres = adres;
            kullanici.IsIhtiyacSahibi = isIhtiyacSahibi;
            kullanici.GuncellenmeTarihi = DateTime.Now;

            // Şifre değişikliği varsa uygula
            if (!string.IsNullOrWhiteSpace(yeniSifre))
            {
                kullanici.SifreHash = HashSifre(yeniSifre);
            }

            await _context.SaveChangesAsync();
            TempData["Mesaj"] = $"\"{ad} {soyad}\" kullanıcısı başarıyla güncellendi.";

            return RedirectToAction(nameof(Kullanicilar));
        }

        // UPDATE - Kullanıcı aktif/pasif durum değiştir (soft delete)
        [HttpPost]
        public async Task<IActionResult> KullaniciDurumDegistir(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Ana admin hesabı devre dışı bırakılamaz
            if (kullanici.KullaniciId == 1)
            {
                TempData["Hata"] = "Ana admin hesabı devre dışı bırakılamaz.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            kullanici.IsActive = !kullanici.IsActive;
            kullanici.GuncellenmeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = kullanici.IsActive
                ? $"\"{kullanici.Ad} {kullanici.Soyad}\" hesabı aktif edildi."
                : $"\"{kullanici.Ad} {kullanici.Soyad}\" hesabı devre dışı bırakıldı.";

            return RedirectToAction(nameof(Kullanicilar));
        }

        // DELETE - Kullanıcı sil (kalıcı)
        [HttpPost]
        public async Task<IActionResult> KullaniciSil(int id)
        {
            var kullanici = await _context.Kullanicilar
                .Include(k => k.Siparisler)
                .Include(k => k.SahipOlduguRestoran)
                .FirstOrDefaultAsync(k => k.KullaniciId == id);

            if (kullanici == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Ana admin hesabı silinemez
            if (kullanici.KullaniciId == 1)
            {
                TempData["Hata"] = "Ana admin hesabı silinemez.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Aktif siparişi olan kullanıcı silinemez
            if (kullanici.Siparisler.Any(s => s.Durum != "TeslimEdildi" && s.Durum != "Iptal"))
            {
                TempData["Hata"] = "Aktif siparişleri olan kullanıcı silinemez.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Restoranı olan kullanıcı silinemez
            if (kullanici.SahipOlduguRestoran != null)
            {
                TempData["Hata"] = "Restoranı olan kullanıcı silinemez. Önce restoranı silin veya devredin.";
                return RedirectToAction(nameof(Kullanicilar));
            }

            // Soft delete
            kullanici.IsActive = false;
            kullanici.GuncellenmeTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Mesaj"] = $"\"{kullanici.Ad} {kullanici.Soyad}\" kullanıcısı silindi (soft-delete).";

            return RedirectToAction(nameof(Kullanicilar));
        }

        // READ - Kullanıcı detay (JSON - AJAX için)
        [HttpGet]
        public async Task<IActionResult> KullaniciDetay(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null) return NotFound();

            return Json(new
            {
                kullanici.KullaniciId,
                kullanici.Ad,
                kullanici.Soyad,
                kullanici.Email,
                kullanici.Telefon,
                kullanici.Rol,
                kullanici.Adres,
                kullanici.IsIhtiyacSahibi,
                kullanici.IsActive,
                OlusturulmaTarihi = kullanici.OlusturulmaTarihi.ToString("dd.MM.yyyy HH:mm"),
                GuncellenmeTarihi = kullanici.GuncellenmeTarihi?.ToString("dd.MM.yyyy HH:mm")
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ASKIDA YEMEK
        // ══════════════════════════════════════════════════════════════════════

        // GET: /Admin/AskidaYemek
        public async Task<IActionResult> AskidaYemek()
        {
            var havuzlar = await _context.AskidaYemekHavuzlari
                .Include(h => h.Restoran)
                .OrderByDescending(h => h.ToplamBakiye)
                .ToListAsync();
            return View(havuzlar);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  YARDIMCI METOTLAR
        // ══════════════════════════════════════════════════════════════════════

        private static string HashSifre(string sifre)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sifre));
            return Convert.ToBase64String(bytes);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisApp.Data;
using YemekSiparisApp.Extensions;
using YemekSiparisApp.Models.Entities;
using YemekSiparisApp.Models.Sepet;
using YemekSiparisApp.Models.ViewModels;

namespace YemekSiparisApp.Controllers
{
    [Authorize]
    public class SiparisController : Controller
    {
        private readonly YemekSiparisContext _context;
        private const string SepetSessionKey = "KullaniciSepeti";
        private const decimal AylikAskidaYemekLimiti = 500m; // TL

        public SiparisController(YemekSiparisContext context)
        {
            _context = context;
        }

        private List<SepetOgesi> SepetiGetir()
        {
            return HttpContext.Session.GetJson<List<SepetOgesi>>(SepetSessionKey) ?? new List<SepetOgesi>();
        }

        // GET: /Siparis/Onay
        [Authorize(Roles = "Musteri")]
        public async Task<IActionResult> Onay()
        {
            if (User.IsInRole("Kurye") || User.IsInRole("Admin") || User.IsInRole("RestoranSahibi"))
            {
                TempData["Hata"] = "Sadece Müşteri hesabı ile sipariş verebilirsiniz.";
                return RedirectToAction("Index", "Home");
            }
            
            var sepet = SepetiGetir();
            if (!sepet.Any())
                return RedirectToAction("Index", "Restoran");

            var model = new SiparisOnayViewModel
            {
                Sepet = new SepetViewModel { Olgeler = sepet }
            };

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Kullanicilar.FindAsync(userId);
                model.TeslimatAdresi = user?.Adres ?? "";

                // Askıda Yemek uygunluk kontrolü
                if (user?.IsIhtiyacSahibi == true)
                {
                    var restoranId = sepet.First().RestoranId;
                    var havuz = await _context.AskidaYemekHavuzlari
                        .FirstOrDefaultAsync(h => h.RestoranId == restoranId && h.IsActive)
                        ?? await _context.AskidaYemekHavuzlari
                            .FirstOrDefaultAsync(h => h.RestoranId == null && h.IsActive);

                    // Bu ay ne kadar kullandı?
                    var ayBasi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var buAyKullanim = await _context.AskidaYemekKullanimlari
                        .Where(k => k.KullaniciId == userId && k.KullanimTarihi >= ayBasi)
                        .SumAsync(k => (decimal?)k.KullanilanMiktar) ?? 0;

                    model.KullaniciIhtiyacSahibi = true;
                    model.KalanAylikLimit = Math.Max(0, AylikAskidaYemekLimiti - buAyKullanim);
                    model.HavuzYeterliBakiye = havuz != null
                        && havuz.ToplamBakiye >= model.Sepet.GenelToplam
                        && model.KalanAylikLimit >= model.Sepet.GenelToplam;
                }
            }

            return View(model);
        }

        // POST: /Siparis/Onay
        [Authorize(Roles = "Musteri")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onay(SiparisOnayViewModel model)
        {
            if (User.IsInRole("Kurye") || User.IsInRole("Admin") || User.IsInRole("RestoranSahibi"))
            {
                return RedirectToAction("Index", "Home");
            }

            var sepet = SepetiGetir();
            if (!sepet.Any()) return RedirectToAction("Index", "Restoran");

            var sepetVm = new SepetViewModel { Olgeler = sepet };
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAskida = model.AskidaYemekKullan;

            // 1. SINIR KONTROLÜ (Aylık Maksimum 500 TL limit)
            if (isAskida)
            {
                var ayBasi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var buAyKullanim = await _context.AskidaYemekKullanimlari
                    .Where(k => k.KullaniciId == userId && k.KullanimTarihi >= ayBasi)
                    .SumAsync(k => (decimal?)k.KullanilanMiktar) ?? 0;

                if (buAyKullanim + sepetVm.GenelToplam > 500)
                {
                    ModelState.AddModelError("", "Aylık Askıda Yemek limitiniz (₺500.00) dolmuştur. Bu siparişi normal ödeme ile verebilirsiniz.");
                    model.Sepet = sepetVm;
                    return View(model);
                }
            }

            // 2. SİPARİŞİ OLUŞTUR
            var yeniSiparis = new Siparis
            {
                MusteriKullaniciId = userId,
                RestoranId = sepet.First().RestoranId,
                Durum = isAskida ? "AskidaOnayBekliyor" : "Beklemede",
                ToplamTutar = sepetVm.GenelToplam,
                TeslimatUcreti = sepetVm.TeslimatUcreti,
                TeslimatAdresi = model.TeslimatAdresi,
                SiparisNotu = model.SiparisNotu,
                OlusturulmaTarihi = DateTime.Now
            };

            foreach (var oge in sepet)
            {
                yeniSiparis.SiparisDetaylari.Add(new SiparisDetay
                {
                    MenuKalemiId = oge.MenuKalemiId,
                    Miktar = oge.Miktar,
                    BirimFiyat = oge.BirimFiyat
                });
            }

            try
            {
                _context.Siparisler.Add(yeniSiparis);
                await _context.SaveChangesAsync();

                // 3. BAĞIŞ SİSTEMİ (Eğer kullanıcı bağış yaptıysa)
                if (!isAskida && model.AskidaYemekDestegiTutari > 0)
                {
                    var havuz = await _context.AskidaYemekHavuzlari.FirstOrDefaultAsync(h => h.IsActive);
                    if (havuz != null)
                    {
                        havuz.ToplamBakiye += model.AskidaYemekDestegiTutari;
                        await _context.SaveChangesAsync();
                    }
                }

                HttpContext.Session.Remove(SepetSessionKey);
                TempData["Mesaj"] = isAskida ? "Askıda yemek talebiniz alındı, admin onayı bekleniyor." : "Siparişiniz başarıyla alındı!";
                return RedirectToAction("Takip", new { id = yeniSiparis.SiparisId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + (ex.InnerException?.Message ?? ex.Message));
                model.Sepet = sepetVm;
                return View(model);
            }
        }

        // GET: /Siparis/Takip/5
        public async Task<IActionResult> Takip(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var siparis = await _context.Siparisler
                .Include(s => s.Restoran)
                .Include(s => s.Kurye)
                    .ThenInclude(k => k.Kullanici)
                .FirstOrDefaultAsync(s => s.SiparisId == id && s.MusteriKullaniciId == userId);

            if (siparis == null) return NotFound();

            return View(siparis);
        }

        // GET: /Siparis/Gecmis
        public async Task<IActionResult> Gecmis()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var siparisler = await _context.Siparisler
                .Include(s => s.Restoran)
                .Include(s => s.SiparisDetaylari)
                .Where(s => s.MusteriKullaniciId == userId)
                .OrderByDescending(s => s.OlusturulmaTarihi)
                .ToListAsync();

            return View(siparisler);
        }
    }
}

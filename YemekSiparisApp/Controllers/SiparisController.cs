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

        public SiparisController(YemekSiparisContext context)
        {
            _context = context;
        }

        private List<SepetOgesi> SepetiGetir()
        {
            return HttpContext.Session.GetJson<List<SepetOgesi>>(SepetSessionKey) ?? new List<SepetOgesi>();
        }

        // GET: /Siparis/Onay
        public async Task<IActionResult> Onay()
        {
            var sepet = SepetiGetir();
            if (!sepet.Any())
            {
                return RedirectToAction("Index", "Restoran");
            }

            var model = new SiparisOnayViewModel
            {
                Sepet = new SepetViewModel { Olgeler = sepet }
            };

            // Kullanıcının kayıtlı adresini çek (varsa)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Kullanicilar.FindAsync(userId);
                model.TeslimatAdresi = user?.Adres ?? "";
            }

            return View(model);
        }

        // POST: /Siparis/Onay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onay(SiparisOnayViewModel model)
        {
            var sepet = SepetiGetir();
            if (!sepet.Any()) return RedirectToAction("Index", "Restoran");

            var sepetVm = new SepetViewModel { Olgeler = sepet };
            if (!ModelState.IsValid)
            {
                model.Sepet = sepetVm;
                return View(model);
            }

            // Siparişi Oluştur
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var restoranId = sepet.First().RestoranId;

            var yeniSiparis = new Siparis
            {
                MusteriKullaniciId = userId,
                RestoranId = restoranId,
                Durum = "Beklemede",
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

            _context.Siparisler.Add(yeniSiparis);
            await _context.SaveChangesAsync(); // SiparisId alır

            // Eğer Askıda Yemek destek seçeneği işaretlendiyse
            if (model.AskidaYemekDestegiTutari > 0)
            {
                // Mevcut restoranın havuzunu bul
                var havuz = await _context.AskidaYemekHavuzlari.FirstOrDefaultAsync(h => h.RestoranId == restoranId && h.IsActive) 
                            ?? await _context.AskidaYemekHavuzlari.FirstOrDefaultAsync(h => h.RestoranId == null && h.IsActive);
                
                if (havuz != null)
                {
                    var bagis = new AskidaYemekBagis
                    {
                        HavuzId = havuz.HavuzId,
                        BagisciKullaniciId = userId,
                        Miktar = model.AskidaYemekDestegiTutari,
                        IsAnonim = false, // Veya formdan alınabilir
                        BagisTarihi = DateTime.Now
                    };
                    _context.AskidaYemekBagislari.Add(bagis);
                    await _context.SaveChangesAsync();
                }
            }

            // Sepeti boşalt
            HttpContext.Session.Remove(SepetSessionKey);

            return RedirectToAction("Takip", new { id = yeniSiparis.SiparisId });
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

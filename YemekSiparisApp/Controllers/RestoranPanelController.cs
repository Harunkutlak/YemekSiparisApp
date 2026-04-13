using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisApp.Data;
using YemekSiparisApp.Models.Entities;

namespace YemekSiparisApp.Controllers
{
    [Authorize(Roles = "RestoranSahibi")]
    public class RestoranPanelController : Controller
    {
        private readonly YemekSiparisContext _context;

        public RestoranPanelController(YemekSiparisContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<Restoran?> GetAktifRestoran()
        {
            var userId = GetUserId();
            return await _context.Restoranlar.FirstOrDefaultAsync(r => r.SahipKullaniciId == userId);
        }

        // GET: /RestoranPanel
        public async Task<IActionResult> Index()
        {
            var r = await GetAktifRestoran();
            if (r == null) return View("RestoranYok");

            var bugun = DateTime.Today;
            var yarib = bugun.AddDays(1);

            var bugunkuSiparisler = await _context.Siparisler
                .Where(s => s.RestoranId == r.RestoranId && s.OlusturulmaTarihi >= bugun && s.OlusturulmaTarihi < yarib)
                .ToListAsync();

            ViewBag.RestoranAdi = r.Ad;
            ViewBag.BugunkuCiro = bugunkuSiparisler.Where(s => s.Durum == "TeslimEdildi").Sum(s => s.ToplamTutar);
            ViewBag.BekleyenSiparisSayisi = bugunkuSiparisler.Count(s => s.Durum == "Beklemede" || s.Durum == "Onaylandi");
            ViewBag.IsActive = r.IsActive;
            
            return View();
        }

        // GET: /RestoranPanel/Siparisler
        public async Task<IActionResult> Siparisler()
        {
            var r = await GetAktifRestoran();
            if (r == null) return View("RestoranYok");

            var siparisler = await _context.Siparisler
                .Include(s => s.MusteriKullanici)
                .Include(s => s.SiparisDetaylari)
                .ThenInclude(sd => sd.MenuKalemi)
                .Where(s => s.RestoranId == r.RestoranId)
                .OrderByDescending(s => s.OlusturulmaTarihi)
                .ToListAsync();

            return View(siparisler);
        }

        [HttpPost]
        public async Task<IActionResult> SiparisDurumGuncelle(int id, string yeniDurum)
        {
            var r = await GetAktifRestoran();
            var siparis = await _context.Siparisler.FirstOrDefaultAsync(s => s.SiparisId == id && s.RestoranId == r!.RestoranId);
            
            if (siparis != null)
            {
                siparis.Durum = yeniDurum;
                
                if (yeniDurum == "TeslimEdildi") siparis.TeslimTarihi = DateTime.Now;
                else if (yeniDurum == "Onaylandi") siparis.OnayTarihi = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Sipariş durumu güncellendi.";
            }

            return RedirectToAction(nameof(Siparisler));
        }

        // GET: /RestoranPanel/Menu
        public async Task<IActionResult> Menu()
        {
            var r = await GetAktifRestoran();
            if (r == null) return View("RestoranYok");

            var menuler = await _context.MenuKalemleri
                .Include(m => m.Kategori)
                .Where(m => m.RestoranId == r.RestoranId && m.IsActive) // Sadece silinmemiş olanlar (Soft Delete)
                .ToListAsync();

            ViewBag.Kategoriler = await _context.Kategoriler.Where(k => k.IsActive).ToListAsync();
            return View(menuler);
        }

        [HttpPost]
        public async Task<IActionResult> MenuEkle(MenuKalemi model)
        {
            var r = await GetAktifRestoran();
            if (r != null)
            {
                model.RestoranId = r.RestoranId;
                model.IsActive = true;
                model.OlusturulmaTarihi = DateTime.Now;

                _context.MenuKalemleri.Add(model);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Ürün eklendi.";
            }
            return RedirectToAction(nameof(Menu));
        }

        [HttpPost]
        public async Task<IActionResult> MenuSatisDurumTogle(int id)
        {
            var r = await GetAktifRestoran();
            var urun = await _context.MenuKalemleri.FirstOrDefaultAsync(m => m.MenuKalemiId == id && m.RestoranId == r!.RestoranId);
            
            if (urun != null)
            {
                urun.IsAkti = !urun.IsAkti; // Satışta mı? durumunu değiştir
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Menu));
        }

        [HttpPost]
        public async Task<IActionResult> MenuSayacSil(int id) // Soft delete
        {
            var r = await GetAktifRestoran();
            var urun = await _context.MenuKalemleri.FirstOrDefaultAsync(m => m.MenuKalemiId == id && m.RestoranId == r!.RestoranId);
            
            if (urun != null)
            {
                urun.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Hata"] = "Ürün menüden kalıcı olarak kaldırıldı.";
            }
            return RedirectToAction(nameof(Menu));
        }
    }
}

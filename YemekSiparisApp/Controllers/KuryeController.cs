using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisApp.Data;
using YemekSiparisApp.Models.Entities;

namespace YemekSiparisApp.Controllers
{
    [Authorize(Roles = "Kurye")]
    public class KuryeController : Controller
    {
        private readonly YemekSiparisContext _context;

        public KuryeController(YemekSiparisContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<Kurye?> GetAktifKurye()
        {
            var userId = GetUserId();
            return await _context.Kuryeler.FirstOrDefaultAsync(k => k.KullaniciId == userId);
        }

        // GET: /Kurye
        public async Task<IActionResult> Index()
        {
            var kurye = await GetAktifKurye();
            if (kurye == null) return Content("Kurye profiliniz bulunamadı.");

            // 1. Alınabilir Siparişler (Hazırlanan ama kuryesi olmayanlar)
            var alinabilirSiparisler = await _context.Siparisler
                .Include(s => s.Restoran)
                .Where(s => s.Durum == "Hazirlaniyor" && s.KuryeId == null)
                .OrderByDescending(s => s.OlusturulmaTarihi)
                .ToListAsync();

            // 2. Üzerimdeki Siparişler (Yolda olanlar)
            var uzerimdekiSiparisler = await _context.Siparisler
                .Include(s => s.Restoran)
                .Where(s => s.KuryeId == kurye.KuryeId && s.Durum == "YoldaKurye")
                .ToListAsync();

            ViewBag.KuryeId = kurye.KuryeId;
            ViewBag.ToplamTeslimat = kurye.ToplamTeslimat;
            ViewBag.Alinabilir = alinabilirSiparisler;
            return View(uzerimdekiSiparisler);
        }

        [HttpPost]
        public async Task<IActionResult> SiparisAl(int id)
        {
            var kurye = await GetAktifKurye();
            if (kurye == null) return NotFound();

            var siparis = await _context.Siparisler.FirstOrDefaultAsync(s => s.SiparisId == id && s.Durum == "Hazirlaniyor" && s.KuryeId == null);
            
            if (siparis != null)
            {
                siparis.KuryeId = kurye.KuryeId;
                siparis.Durum = "YoldaKurye";
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Siparişi üzerinize aldınız. Şimdi teslimat zamanı!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TeslimEt(int id)
        {
            var kurye = await GetAktifKurye();
            var siparis = await _context.Siparisler.FirstOrDefaultAsync(s => s.SiparisId == id && s.KuryeId == kurye!.KuryeId);

            if (siparis != null)
            {
                siparis.Durum = "TeslimEdildi";
                siparis.TeslimTarihi = DateTime.Now;
                
                // Kurye istatistiklerini artır
                kurye!.ToplamTeslimat++;
                
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Tebrikler! Sipariş teslim edildi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

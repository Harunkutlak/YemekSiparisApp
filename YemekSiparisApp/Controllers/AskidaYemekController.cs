using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YemekSiparisApp.Data;
using YemekSiparisApp.Models.Entities;

namespace YemekSiparisApp.Controllers
{
    public class AskidaYemekController : Controller
    {
        private readonly YemekSiparisContext _context;

        public AskidaYemekController(YemekSiparisContext context)
        {
            _context = context;
        }

        // GET: /AskidaYemek
        public async Task<IActionResult> Index()
        {
            var havuz = await _context.AskidaYemekHavuzlari.FirstOrDefaultAsync(h => h.IsActive);
            if (havuz == null)
            {
                TempData["Mesaj"] = "Şu anda aktif bir Askıda Yemek havuzu bulunmuyor.";
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.SonBagislar = await _context.AskidaYemekBagislari
                .Include(b => b.BagisciKullanici)
                .Where(b => b.HavuzId == havuz.HavuzId)
                .OrderByDescending(b => b.BagisTarihi)
                .Take(10)
                .ToListAsync();

            return View(havuz);
        }

        // GET: /AskidaYemek/Bagis
        public IActionResult Bagis()
        {
            return View();
        }

        // POST: /AskidaYemek/Bagis
        [HttpPost]
        public async Task<IActionResult> Bagis(decimal miktar, bool isAnonim, string mesaj)
        {
            if (miktar <= 0)
            {
                ModelState.AddModelError("", "Geçerli bir bağış miktarı giriniz.");
                return View();
            }

            var havuz = await _context.AskidaYemekHavuzlari.FirstOrDefaultAsync(h => h.IsActive);
            if (havuz == null)
            {
                TempData["Mesaj"] = "Şu anda bağış kabul eden bir havuz yok.";
                return RedirectToAction("Index", "Home");
            }

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr)) userId = int.Parse(userIdStr);
            }
            else
            {
                // Giriş yoksa zorunlu anonim.
                isAnonim = true;
            }

            var bagis = new AskidaYemekBagis
            {
                BagisciKullaniciId = isAnonim ? null : userId,
                HavuzId = havuz.HavuzId,
                Miktar = miktar,
                IsAnonim = isAnonim,
                Mesaj = mesaj,
                BagisTarihi = DateTime.Now
            };

            _context.AskidaYemekBagislari.Add(bagis);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = $"Teşekkürler! ₺{miktar:N2} tutarındaki bağışınız havuza ulaştı.";
            return RedirectToAction("Index");
        }
    }
}

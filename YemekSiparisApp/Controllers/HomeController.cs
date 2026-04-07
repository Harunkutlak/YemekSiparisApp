using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YemekSiparisApp.Data;

namespace YemekSiparisApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly YemekSiparisContext _context;

        public HomeController(YemekSiparisContext context)
        {
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            // Aktif restoranları yükle (en yüksek puanlıdan)
            var restoranlar = await _context.Restoranlar
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Puan)
                .Take(6)
                .ToListAsync();

            // Askıda Yemek havuz durumu
            var havuzBakiye = await _context.AskidaYemekHavuzlari
                .Where(h => h.IsActive)
                .SumAsync(h => h.ToplamBakiye);

            ViewBag.Restoranlar = restoranlar;
            ViewBag.HavuzBakiye = havuzBakiye;

            return View();
        }

        public IActionResult Hakkimizda() => View();

        public IActionResult Hata()
        {
            return View();
        }
    }
}

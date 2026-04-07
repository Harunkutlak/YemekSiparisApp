using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YemekSiparisApp.Data;

namespace YemekSiparisApp.Controllers
{
    public class RestoranController : Controller
    {
        private readonly YemekSiparisContext _context;

        public RestoranController(YemekSiparisContext context)
        {
            _context = context;
        }

        // GET: /Restoran  — tüm aktif restoranları listele
        public async Task<IActionResult> Index(string? arama, int? kategoriId)
        {
            var restoranlar = _context.Restoranlar
                .Where(r => r.IsActive);

            if (!string.IsNullOrWhiteSpace(arama))
                restoranlar = restoranlar.Where(r => r.Ad.Contains(arama));

            ViewBag.Kategoriler = await _context.Kategoriler
                .Where(k => k.IsActive).ToListAsync();

            return View(await restoranlar.OrderByDescending(r => r.Puan).ToListAsync());
        }

        // GET: /Restoran/Detay/5  — restoran menüsü
        public async Task<IActionResult> Detay(int id)
        {
            var restoran = await _context.Restoranlar
                .Include(r => r.MenuKalemleri.Where(m => m.IsActive && m.IsAkti))
                    .ThenInclude(m => m.Kategori)
                .FirstOrDefaultAsync(r => r.RestoranId == id && r.IsActive);

            if (restoran == null)
                return NotFound();

            return View(restoran);
        }
    }
}

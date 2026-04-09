using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YemekSiparisApp.Data;
using YemekSiparisApp.Extensions;
using YemekSiparisApp.Models.Sepet;
using YemekSiparisApp.Models.ViewModels;

namespace YemekSiparisApp.Controllers
{
    public class SepetController : Controller
    {
        private readonly YemekSiparisContext _context;
        private const string SepetSessionKey = "KullaniciSepeti";

        public SepetController(YemekSiparisContext context)
        {
            _context = context;
        }

        private List<SepetOgesi> SepetiGetir()
        {
            return HttpContext.Session.GetJson<List<SepetOgesi>>(SepetSessionKey) ?? new List<SepetOgesi>();
        }

        private void SepetiKaydet(List<SepetOgesi> sepet)
        {
            HttpContext.Session.SetJson(SepetSessionKey, sepet);
        }

        // GET: /Sepet
        public IActionResult Index()
        {
            var sepet = SepetiGetir();
            var model = new SepetViewModel { Olgeler = sepet };
            return View(model);
        }

        // POST: /Sepet/Ekle
        [HttpPost]
        public async Task<IActionResult> Ekle(int menuKalemiId)
        {
            var urun = await _context.MenuKalemleri
                .Include(m => m.Restoran)
                .FirstOrDefaultAsync(m => m.MenuKalemiId == menuKalemiId && m.IsActive && m.IsAkti);

            if (urun == null)
            {
                TempData["Mesaj"] = "Ürün bulunamadı veya şu an satışta değil.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/Restoran");
            }

            var sepet = SepetiGetir();

            // Farklı restorandan ürün eklenmeye çalışılıyorsa engelle
            if (sepet.Any() && sepet.First().RestoranId != urun.RestoranId)
            {
                // Alternatif olarak sepeti boşaltıp eklenebilir, ancak şimdilik hata verelim.
                TempData["Mesaj"] = "Aynı anda sadece bir restorandan sipariş verebilirsiniz. Lütfen önce sepetinizi boşaltın.";
                return RedirectToAction("Detay", "Restoran", new { id = urun.RestoranId });
            }

            var sepetOgesi = sepet.FirstOrDefault(o => o.MenuKalemiId == menuKalemiId);
            if (sepetOgesi != null)
            {
                sepetOgesi.Miktar++;
            }
            else
            {
                sepet.Add(new SepetOgesi
                {
                    MenuKalemiId = urun.MenuKalemiId,
                    UrunAdi = urun.Ad,
                    BirimFiyat = urun.Fiyat,
                    Miktar = 1,
                    RestoranId = urun.RestoranId,
                    RestoranAdi = urun.Restoran!.Ad
                });
            }

            SepetiKaydet(sepet);
            TempData["Mesaj"] = $"{urun.Ad} sepete eklendi!";
            
            // Kullanıcıyı geldiği sayfaya geri gönder
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Restoran");
        }

        // POST: /Sepet/Kaldir
        [HttpPost]
        public IActionResult Kaldir(int menuKalemiId)
        {
            var sepet = SepetiGetir();
            var sepetOgesi = sepet.FirstOrDefault(o => o.MenuKalemiId == menuKalemiId);
            
            if (sepetOgesi != null)
            {
                if (sepetOgesi.Miktar > 1)
                {
                    sepetOgesi.Miktar--;
                }
                else
                {
                    sepet.Remove(sepetOgesi);
                }
                SepetiKaydet(sepet);
            }

            return RedirectToAction("Index");
        }

        // GET/POST: /Sepet/Bosalt
        public IActionResult Bosalt()
        {
            HttpContext.Session.Remove(SepetSessionKey);
            return RedirectToAction("Index", "Restoran");
        }
    }
}

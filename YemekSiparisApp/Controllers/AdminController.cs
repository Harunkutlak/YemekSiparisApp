using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.ToplamKullanici = await _context.Kullanicilar.CountAsync();
            ViewBag.ToplamSiparis = await _context.Siparisler.CountAsync();
            ViewBag.ToplamRestoran = await _context.Restoranlar.CountAsync();
            ViewBag.ToplamHavuzBakiye = await _context.AskidaYemekHavuzlari.SumAsync(h => h.ToplamBakiye);

            return View();
        }

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

            return View(restoranlar);
        }

        [HttpPost]
        public async Task<IActionResult> RestoranEkle(Restoran model)
        {
            if (ModelState.IsValid)
            {
                model.IsActive = true;
                model.ToplamCiro = 0;
                model.PuanSayisi = 0;
                
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

        [HttpPost]
        public async Task<IActionResult> RestoranDurumDegistir(int id)
        {
            var restoran = await _context.Restoranlar.FindAsync(id);
            if (restoran != null)
            {
                restoran.IsActive = !restoran.IsActive;
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Restoran durumu güncellendi.";
            }
            return RedirectToAction(nameof(Restoranlar));
        }

        // GET: /Admin/Kullanicilar
        public async Task<IActionResult> Kullanicilar()
        {
            var kullanicilar = await _context.Kullanicilar
                .OrderByDescending(k => k.KullaniciId)
                .ToListAsync();
            return View(kullanicilar);
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciGuncelle(int id, string rol, bool isIhtiyacSahibi)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici != null && kullanici.KullaniciId != 1) // Admin kendi rolünü bozmasın diye ufak koruma
            {
                kullanici.Rol = rol;
                kullanici.IsIhtiyacSahibi = isIhtiyacSahibi;
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Kullanıcı başarıyla güncellendi.";
            }
            return RedirectToAction(nameof(Kullanicilar));
        }

        // GET: /Admin/AskidaYemek
        public async Task<IActionResult> AskidaYemek()
        {
            var havuzlar = await _context.AskidaYemekHavuzlari
                .Include(h => h.Restoran)
                .OrderByDescending(h => h.ToplamBakiye)
                .ToListAsync();
            return View(havuzlar);
        }
    }
}

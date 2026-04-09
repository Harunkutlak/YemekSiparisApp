using YemekSiparisApp.Models.Sepet;

namespace YemekSiparisApp.Models.ViewModels
{
    public class SepetViewModel
    {
        public List<SepetOgesi> Olgeler { get; set; } = new List<SepetOgesi>();
        
        public decimal AraToplam => Olgeler.Sum(o => o.ToplamTutar);
        public decimal TeslimatUcreti { get; set; } = 0; // Şimdilik 0
        public decimal GenelToplam => AraToplam + TeslimatUcreti;
        
        public int? AktifRestoranId => Olgeler.FirstOrDefault()?.RestoranId;
        public string? AktifRestoranAdi => Olgeler.FirstOrDefault()?.RestoranAdi;
    }
}

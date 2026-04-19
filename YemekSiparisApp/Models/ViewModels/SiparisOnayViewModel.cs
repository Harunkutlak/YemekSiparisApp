using System.ComponentModel.DataAnnotations;

namespace YemekSiparisApp.Models.ViewModels
{
    public class SiparisOnayViewModel
    {
        public SepetViewModel Sepet { get; set; } = new SepetViewModel();

        [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
        [MaxLength(400)]
        public string TeslimatAdresi { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? SiparisNotu { get; set; }

        public decimal AskidaYemekDestegiTutari { get; set; } = 0;

        // Kullanıcı Askıda Yemek fonundan ücretsiz sipariş vermek istiyor mu?
        public bool AskidaYemekKullan { get; set; } = false;

        // View'a bilgi aktarımı için (controller'dan doldurulur)
        public bool KullaniciIhtiyacSahibi { get; set; } = false;
        public bool HavuzYeterliBakiye { get; set; } = false;
        public decimal KalanAylikLimit { get; set; } = 0;
    }
}


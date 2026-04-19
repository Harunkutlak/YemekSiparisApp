using System.ComponentModel.DataAnnotations;

namespace YemekSiparisApp.Models.ViewModels
{
    public class ProfilViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur."), MaxLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur."), MaxLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon zorunludur."), MaxLength(15)]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Telefon { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Adres { get; set; }

        // ── Şifre değişikliği (opsiyonel) ─────────────────────────
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string? MevcutSifre { get; set; }

        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string? YeniSifre { get; set; }

        [DataType(DataType.Password)]
        [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? YeniSifreTekrar { get; set; }

        // ── Sadece görüntüleme (readonly) ──────────────────────────
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime OlusturulmaTarihi { get; set; }
        public int ToplamSiparis { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace YemekSiparisApp.Models.ViewModels
{
    public class GirisViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;

        public bool BeniHatirla { get; set; }
    }

    public class KayitViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur."), MaxLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur."), MaxLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string SifreTekrar { get; set; } = string.Empty;

        public string? Adres { get; set; }
    }
}

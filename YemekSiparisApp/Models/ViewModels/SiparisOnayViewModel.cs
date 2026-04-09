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
    }
}

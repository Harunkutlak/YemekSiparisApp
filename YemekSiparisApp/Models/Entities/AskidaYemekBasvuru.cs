using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Kullanıcının Askıda Yemek programından yararlanmak için yaptığı başvuru.
    /// Admin onaylarsa IsIhtiyacSahibi = true yapılır.
    /// </summary>
    public class AskidaYemekBasvuru
    {
        [Key]
        public int BasvuruId { get; set; }

        [ForeignKey("Kullanici")]
        public int KullaniciId { get; set; }

        /// <summary>
        /// Beklemede | Onaylandi | Reddedildi
        /// </summary>
        [Required, MaxLength(20)]
        public string Durum { get; set; } = "Beklemede";

        /// <summary>
        /// Kullanıcının yazdığı başvuru notu (neden ihtiyaç duyduğunu açıklar)
        /// </summary>
        [MaxLength(500)]
        public string? BasvuruNotu { get; set; }

        /// <summary>
        /// Adminin yazdığı değerlendirme notu
        /// </summary>
        [MaxLength(300)]
        public string? AdminNotu { get; set; }

        public DateTime BasvuruTarihi { get; set; } = DateTime.Now;
        public DateTime? IslemTarihi { get; set; }

        // Navigation Property
        public virtual Kullanici Kullanici { get; set; } = null!;
    }
}

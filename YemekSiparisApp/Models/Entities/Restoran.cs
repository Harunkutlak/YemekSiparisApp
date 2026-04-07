using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Restoranları temsil eder — CHECK kısıtı: 1 ≤ Puan ≤ 5
    /// </summary>
    public class Restoran
    {
        [Key]
        public int RestoranId { get; set; }

        [ForeignKey("SahipKullanici")]
        public int SahipKullaniciId { get; set; }

        [Required, MaxLength(100)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Aciklama { get; set; }

        [Required, MaxLength(300)]
        public string Adres { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Telefon { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// CHECK: Puan >= 1 AND Puan <= 5 (DB'de constraint ile sağlanır)
        /// </summary>
        [Column(TypeName = "decimal(3,2)")]
        public decimal? Puan { get; set; }

        public int PuanSayisi { get; set; } = 0;

        /// <summary>
        /// Teslim edilen siparişlerden sonra otomatik güncellenir (Trigger ile)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamCiro { get; set; } = 0;

        public int MinSiparisUcreti { get; set; } = 0;

        public int TahminiTeslimatDakika { get; set; } = 30;

        /// <summary>
        /// Restoranın çalıştığı şehir/ilçe
        /// </summary>
        [MaxLength(50)]
        public string? Sehir { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Kullanici SahipKullanici { get; set; } = null!;
        public virtual ICollection<MenuKalemi> MenuKalemleri { get; set; } = new List<MenuKalemi>();
        public virtual ICollection<Siparis> Siparisler { get; set; } = new List<Siparis>();
    }
}

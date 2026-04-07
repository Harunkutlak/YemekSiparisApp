using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Restoranların menü kalemleri — CHECK: Fiyat > 0
    /// Soft Delete uygulanır: IsActive = false ile silinir
    /// </summary>
    public class MenuKalemi
    {
        [Key]
        public int MenuKalemiId { get; set; }

        [ForeignKey("Restoran")]
        public int RestoranId { get; set; }

        [ForeignKey("Kategori")]
        public int KategoriId { get; set; }

        [Required, MaxLength(100)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Aciklama { get; set; }

        /// <summary>
        /// CHECK: Fiyat > 0 (DB constraint'i ile sağlanır)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fiyat { get; set; }

        [MaxLength(200)]
        public string? ResimUrl { get; set; }

        public int HazirlamaSuresi { get; set; } = 15; // Dakika

        public bool IsVejetaryen { get; set; } = false;
        public bool IsAkti { get; set; } = true; // Bugün satışta mı?

        /// <summary>
        /// Soft Delete: false = menüden kaldırıldı, veri silinmez
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellenmeTarihi { get; set; }

        // Navigation Properties
        public virtual Restoran Restoran { get; set; } = null!;
        public virtual Kategori Kategori { get; set; } = null!;
        public virtual ICollection<SiparisDetay> SiparisDetaylari { get; set; } = new List<SiparisDetay>();
    }
}

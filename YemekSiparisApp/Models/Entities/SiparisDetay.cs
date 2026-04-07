using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Sipariş detay satırları — sipariş anındaki fiyat snapshot'ı tutulur (3NF)
    /// </summary>
    public class SiparisDetay
    {
        [Key]
        public int SiparisDetayId { get; set; }

        [ForeignKey("Siparis")]
        public int SiparisId { get; set; }

        [ForeignKey("MenuKalemi")]
        public int MenuKalemiId { get; set; }

        public int Miktar { get; set; } = 1;

        /// <summary>
        /// Sipariş anındaki fiyat (MenuKalemi.Fiyat değişse de bu değer korunur)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal BirimFiyat { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ToplamFiyat { get; set; } // Miktar * BirimFiyat

        [MaxLength(200)]
        public string? Notlar { get; set; }

        // Navigation Properties
        public virtual Siparis Siparis { get; set; } = null!;
        public virtual MenuKalemi MenuKalemi { get; set; } = null!;
    }
}

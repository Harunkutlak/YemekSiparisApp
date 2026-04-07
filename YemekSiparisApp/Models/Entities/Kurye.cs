using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Kurye bilgileri — Kullanici tablosuna 1:1 ilişki
    /// </summary>
    public class Kurye
    {
        [Key]
        public int KuryeId { get; set; }

        [ForeignKey("Kullanici")]
        public int KullaniciId { get; set; }

        [MaxLength(20)]
        public string? Plaka { get; set; }

        [MaxLength(30)]
        public string? AracTipi { get; set; } // Motosiklet | Bisiklet | Araba

        /// <summary>
        /// Musait | Mesgul | Offline
        /// </summary>
        [MaxLength(20)]
        public string Durum { get; set; } = "Offline";

        public int ToplamTeslimat { get; set; } = 0;

        [Column(TypeName = "decimal(3,2)")]
        public decimal? Puan { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Kullanici Kullanici { get; set; } = null!;
        public virtual ICollection<Siparis> Siparisler { get; set; } = new List<Siparis>();
    }
}

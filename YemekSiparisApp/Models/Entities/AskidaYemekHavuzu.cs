using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Askıda Yemek Havuzu — bağışların biriktiği merkezi kasa.
    /// Her restoran için ayrı veya genel havuz açılabilir.
    /// </summary>
    public class AskidaYemekHavuzu
    {
        [Key]
        public int HavuzId { get; set; }

        [Required, MaxLength(100)]
        public string Ad { get; set; } = "Genel Havuz";

        [MaxLength(300)]
        public string? Aciklama { get; set; }

        /// <summary>
        /// Null ise genel havuz, dolu ise restoran bazlı havuz
        /// </summary>
        [ForeignKey("Restoran")]
        public int? RestoranId { get; set; }

        /// <summary>
        /// Güncel bakiye — bağışlar ekler, kullanımlar düşer (Trigger ile otomatik)
        /// </summary>
        [Column(TypeName = "decimal(12,2)")]
        public decimal ToplamBakiye { get; set; } = 0;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellenmeTarihi { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Restoran? Restoran { get; set; }
        public virtual ICollection<AskidaYemekBagis> Bagislar { get; set; } = new List<AskidaYemekBagis>();
        public virtual ICollection<AskidaYemekKullanim> Kullanimlar { get; set; } = new List<AskidaYemekKullanim>();
    }
}

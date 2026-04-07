using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Askıda Yemek Kullanımları — ihtiyaç sahibi kullanıcıların havuzdan ücretsiz sipariş vermesi.
    /// Sipariş "TeslimEdildi" statüsüne geçince Trigger ile ToplamBakiye otomatik düşer.
    /// </summary>
    public class AskidaYemekKullanim
    {
        [Key]
        public int KullanimId { get; set; }

        [ForeignKey("KullaniciKullanici")]
        public int KullaniciId { get; set; }

        [ForeignKey("Siparis")]
        public int SiparisId { get; set; }

        [ForeignKey("Havuz")]
        public int HavuzId { get; set; }

        /// <summary>
        /// Havuzdan düşülen tutar — Sipariş tamamlandığında Trigger tarafından işlenir
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal KullanilanMiktar { get; set; }

        public DateTime KullanimTarihi { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? Aciklama { get; set; }

        // Navigation Properties
        public virtual Kullanici KullaniciKullanici { get; set; } = null!;
        public virtual Siparis Siparis { get; set; } = null!;
        public virtual AskidaYemekHavuzu Havuz { get; set; } = null!;
    }
}

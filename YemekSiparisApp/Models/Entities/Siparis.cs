using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Siparişler — CHECK: ToplamTutar > 0
    /// IsAskidaYemek = true ise Askıda Yemek havuzundan karşılanır
    /// </summary>
    public class Siparis
    {
        [Key]
        public int SiparisId { get; set; }

        [ForeignKey("MusteriKullanici")]
        public int MusteriKullaniciId { get; set; }

        [ForeignKey("Restoran")]
        public int RestoranId { get; set; }

        [ForeignKey("Kurye")]
        public int? KuryeId { get; set; } // Atanmadan önce null

        /// <summary>
        /// Sipariş durumu
        /// Beklemede | Onaylandi | Hazirlaniyor | YoldaKurye | TeslimEdildi | Iptal
        /// </summary>
        [Required, MaxLength(30)]
        public string Durum { get; set; } = "Beklemede";

        /// <summary>
        /// CHECK: ToplamTutar > 0
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal ToplamTutar { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TeslimatUcreti { get; set; } = 0;

        [Required, MaxLength(400)]
        public string TeslimatAdresi { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? SiparisNotu { get; set; }

        /// <summary>
        /// Askıda Yemek modülü: Bu sipariş havuzdan mı karşılandı?
        /// </summary>
        public bool IsAskidaYemek { get; set; } = false;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        public DateTime? OnayTarihi { get; set; }
        public DateTime? TeslimTarihi { get; set; }

        // Navigation Properties
        public virtual Kullanici MusteriKullanici { get; set; } = null!;
        public virtual Restoran Restoran { get; set; } = null!;
        public virtual Kurye? Kurye { get; set; }
        public virtual ICollection<SiparisDetay> SiparisDetaylari { get; set; } = new List<SiparisDetay>();
        public virtual AskidaYemekKullanim? AskidaYemekKullanimi { get; set; }
    }
}

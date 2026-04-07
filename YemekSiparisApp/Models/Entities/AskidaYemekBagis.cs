using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Askıda Yemek Bağışları — hayırsever müşterilerin havuza yaptığı bağışlar.
    /// Anonim bağış desteklenir: BagisciKullaniciId null olabilir.
    /// CHECK: Miktar > 0
    /// </summary>
    public class AskidaYemekBagis
    {
        [Key]
        public int BagisId { get; set; }


        /// <summary>
        /// Anonim bağış: null ise kimlik gizlendi
        /// </summary>
        [ForeignKey("BagisciKullanici")]
        public int? BagisciKullaniciId { get; set; }

        [ForeignKey("Havuz")]
        public int HavuzId { get; set; }

        /// <summary>
        /// CHECK: Miktar > 0 (DB constraint'i ile sağlanır)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal Miktar { get; set; }

        /// <summary>
        /// true = anonim bağış (isim gizlenir), false = açık bağış
        /// </summary>
        public bool IsAnonim { get; set; } = false;

        [MaxLength(300)]
        public string? Mesaj { get; set; }

        public DateTime BagisTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Kullanici? BagisciKullanici { get; set; }
        public virtual AskidaYemekHavuzu Havuz { get; set; } = null!;
    }
}

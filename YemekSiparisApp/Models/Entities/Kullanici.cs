using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Sistemdeki tüm kullanıcıları temsil eder (Müşteri, Admin, Restoran Sahibi, Kurye)
    /// </summary>
    public class Kullanici
    {
        [Key]
        public int KullaniciId { get; set; }

        [Required, MaxLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;   // UNIQUE - DB'de constraint ile

        [Required, MaxLength(15)]
        public string Telefon { get; set; } = string.Empty; // UNIQUE - DB'de constraint ile

        [Required, MaxLength(256)]
        public string SifreHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Rol { get; set; } = "Musteri"; // Musteri | Admin | RestoranSahibi | Kurye

        [MaxLength(250)]
        public string? Adres { get; set; }

        /// <summary>
        /// Askıda Yemek modülü: İhtiyaç sahibi olarak doğrulanmış mı?
        /// </summary>
        public bool IsIhtiyacSahibi { get; set; } = false;

        /// <summary>
        /// Soft Delete: false ise kullanıcı pasif/silinmiş sayılır
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellenmeTarihi { get; set; }

        // Navigation Properties
        public virtual ICollection<Siparis> Siparisler { get; set; } = new List<Siparis>();
        public virtual ICollection<AskidaYemekBagis> AskidaYemekBagislari { get; set; } = new List<AskidaYemekBagis>();
        public virtual ICollection<AskidaYemekKullanim> AskidaYemekKullanimlari { get; set; } = new List<AskidaYemekKullanim>();
        public virtual Restoran? SahipOlduguRestoran { get; set; }
        public virtual Kurye? KuryeBilgisi { get; set; }
    }
}

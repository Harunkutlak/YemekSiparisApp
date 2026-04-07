using System.ComponentModel.DataAnnotations;

namespace YemekSiparisApp.Models.Entities
{
    /// <summary>
    /// Menü kategorileri (Pizzalar, Burgerler, İçecekler vb.)
    /// </summary>
    public class Kategori
    {
        [Key]
        public int KategoriId { get; set; }

        [Required, MaxLength(80)]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Aciklama { get; set; }

        [MaxLength(100)]
        public string? IkonUrl { get; set; }

        public int SiralamaNo { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<MenuKalemi> MenuKalemleri { get; set; } = new List<MenuKalemi>();
    }
}

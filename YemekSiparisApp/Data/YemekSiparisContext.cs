using Microsoft.EntityFrameworkCore;
using YemekSiparisApp.Models.Entities;

namespace YemekSiparisApp.Data
{
    public class YemekSiparisContext : DbContext
    {
        public YemekSiparisContext(DbContextOptions<YemekSiparisContext> options)
            : base(options) { }

        // ── DbSet'ler (tablolar) ────────────────────────────────────────────
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Restoran> Restoranlar { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<MenuKalemi> MenuKalemleri { get; set; }
        public DbSet<Kurye> Kuryeler { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }
        public DbSet<SiparisDetay> SiparisDetaylari { get; set; }
        public DbSet<AskidaYemekHavuzu> AskidaYemekHavuzlari { get; set; }
        public DbSet<AskidaYemekBagis> AskidaYemekBagislari { get; set; }
        public DbSet<AskidaYemekKullanim> AskidaYemekKullanimlari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── KULLANICI ────────────────────────────────────────────────────
            modelBuilder.Entity<Kullanici>(entity =>
            {
                entity.ToTable("Kullanicilar");
                entity.HasIndex(k => k.Email).IsUnique();       // UNIQUE Email
                entity.HasIndex(k => k.Telefon).IsUnique();     // UNIQUE Telefon
                entity.HasCheckConstraint("CK_Kullanicilar_Rol",
                    "Rol IN ('Musteri','Admin','RestoranSahibi','Kurye')");
            });

            // ── RESTORAN ─────────────────────────────────────────────────────
            modelBuilder.Entity<Restoran>(entity =>
            {
                entity.ToTable("Restoranlar");
                // CHECK: 1 ≤ Puan ≤ 5
                entity.HasCheckConstraint("CK_Restoranlar_Puan",
                    "Puan IS NULL OR (Puan >= 1.0 AND Puan <= 5.0)");
                // CHECK: ToplamCiro >= 0
                entity.HasCheckConstraint("CK_Restoranlar_ToplamCiro",
                    "ToplamCiro >= 0");

                entity.HasOne(r => r.SahipKullanici)
                      .WithOne(k => k.SahipOlduguRestoran)
                      .HasForeignKey<Restoran>(r => r.SahipKullaniciId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── MENÜKALEMİ ───────────────────────────────────────────────────
            modelBuilder.Entity<MenuKalemi>(entity =>
            {
                entity.ToTable("MenuKalemleri");
                // CHECK: Fiyat > 0
                entity.HasCheckConstraint("CK_MenuKalemleri_Fiyat", "Fiyat > 0");

                entity.HasOne(m => m.Restoran)
                      .WithMany(r => r.MenuKalemleri)
                      .HasForeignKey(m => m.RestoranId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Kategori)
                      .WithMany(k => k.MenuKalemleri)
                      .HasForeignKey(m => m.KategoriId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── KURYE ────────────────────────────────────────────────────────
            modelBuilder.Entity<Kurye>(entity =>
            {
                entity.ToTable("Kuryeler");
                entity.HasCheckConstraint("CK_Kuryeler_Durum",
                    "Durum IN ('Musait','Mesgul','Offline')");

                entity.HasOne(k => k.Kullanici)
                      .WithOne(u => u.KuryeBilgisi)
                      .HasForeignKey<Kurye>(k => k.KullaniciId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── SİPARİŞ ──────────────────────────────────────────────────────
            modelBuilder.Entity<Siparis>(entity =>
            {
                entity.ToTable("Siparisler");
                // CHECK: ToplamTutar > 0
                entity.HasCheckConstraint("CK_Siparisler_ToplamTutar", "ToplamTutar > 0");
                entity.HasCheckConstraint("CK_Siparisler_Durum",
                    "Durum IN ('Beklemede','Onaylandi','Hazirlaniyor','YoldaKurye','TeslimEdildi','Iptal')");

                entity.HasOne(s => s.MusteriKullanici)
                      .WithMany(k => k.Siparisler)
                      .HasForeignKey(s => s.MusteriKullaniciId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Restoran)
                      .WithMany(r => r.Siparisler)
                      .HasForeignKey(s => s.RestoranId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Kurye)
                      .WithMany(k => k.Siparisler)
                      .HasForeignKey(s => s.KuryeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── SİPARİŞ DETAY ─────────────────────────────────────────────
            modelBuilder.Entity<SiparisDetay>(entity =>
            {
                entity.ToTable("SiparisDetaylari");
                entity.HasCheckConstraint("CK_SiparisDetay_Miktar", "Miktar > 0");

                entity.HasOne(sd => sd.Siparis)
                      .WithMany(s => s.SiparisDetaylari)
                      .HasForeignKey(sd => sd.SiparisId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sd => sd.MenuKalemi)
                      .WithMany(m => m.SiparisDetaylari)
                      .HasForeignKey(sd => sd.MenuKalemiId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ASKIDA YEMEK HAVUZU ───────────────────────────────────────
            modelBuilder.Entity<AskidaYemekHavuzu>(entity =>
            {
                entity.ToTable("AskidaYemekHavuzlari");
                entity.HasCheckConstraint("CK_Havuz_Bakiye", "ToplamBakiye >= 0");

                entity.HasOne(h => h.Restoran)
                      .WithMany()
                      .HasForeignKey(h => h.RestoranId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── ASKIDA YEMEK BAĞIŞ ───────────────────────────────────────
            modelBuilder.Entity<AskidaYemekBagis>(entity =>
            {
                entity.ToTable("AskidaYemekBagislari");
                // CHECK: Miktar > 0
                entity.HasCheckConstraint("CK_Bagis_Miktar", "Miktar > 0");

                entity.HasOne(b => b.BagisciKullanici)
                      .WithMany(k => k.AskidaYemekBagislari)
                      .HasForeignKey(b => b.BagisciKullaniciId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(b => b.Havuz)
                      .WithMany(h => h.Bagislar)
                      .HasForeignKey(b => b.HavuzId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ASKIDA YEMEK KULLANIM ─────────────────────────────────────
            modelBuilder.Entity<AskidaYemekKullanim>(entity =>
            {
                entity.ToTable("AskidaYemekKullanimlari");
                entity.HasCheckConstraint("CK_Kullanim_Miktar", "KullanilanMiktar > 0");

                entity.HasOne(ku => ku.KullaniciKullanici)
                      .WithMany(k => k.AskidaYemekKullanimlari)
                      .HasForeignKey(ku => ku.KullaniciId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ku => ku.Siparis)
                      .WithOne(s => s.AskidaYemekKullanimi)
                      .HasForeignKey<AskidaYemekKullanim>(ku => ku.SiparisId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ku => ku.Havuz)
                      .WithMany(h => h.Kullanimlar)
                      .HasForeignKey(ku => ku.HavuzId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

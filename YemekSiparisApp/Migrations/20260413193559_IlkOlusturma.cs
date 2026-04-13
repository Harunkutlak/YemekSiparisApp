using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YemekSiparisApp.Migrations
{
    /// <inheritdoc />
    public partial class IlkOlusturma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    KategoriId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IkonUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SiralamaNo = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.KategoriId);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsIhtiyacSahibi = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciId);
                    table.CheckConstraint("CK_Kullanicilar_Rol", "Rol IN ('Musteri','Admin','RestoranSahibi','Kurye')");
                });

            migrationBuilder.CreateTable(
                name: "Kuryeler",
                columns: table => new
                {
                    KuryeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    Plaka = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AracTipi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToplamTeslimat = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kuryeler", x => x.KuryeId);
                    table.CheckConstraint("CK_Kuryeler_Durum", "Durum IN ('Musait','Mesgul','Offline')");
                    table.ForeignKey(
                        name: "FK_Kuryeler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Restoranlar",
                columns: table => new
                {
                    RestoranId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SahipKullaniciId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Puan = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    PuanSayisi = table.Column<int>(type: "int", nullable: false),
                    ToplamCiro = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinSiparisUcreti = table.Column<int>(type: "int", nullable: false),
                    TahminiTeslimatDakika = table.Column<int>(type: "int", nullable: false),
                    Sehir = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restoranlar", x => x.RestoranId);
                    table.CheckConstraint("CK_Restoranlar_Puan", "Puan IS NULL OR (Puan >= 1.0 AND Puan <= 5.0)");
                    table.CheckConstraint("CK_Restoranlar_ToplamCiro", "ToplamCiro >= 0");
                    table.ForeignKey(
                        name: "FK_Restoranlar_Kullanicilar_SahipKullaniciId",
                        column: x => x.SahipKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AskidaYemekHavuzlari",
                columns: table => new
                {
                    HavuzId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RestoranId = table.Column<int>(type: "int", nullable: true),
                    ToplamBakiye = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AskidaYemekHavuzlari", x => x.HavuzId);
                    table.CheckConstraint("CK_Havuz_Bakiye", "ToplamBakiye >= 0");
                    table.ForeignKey(
                        name: "FK_AskidaYemekHavuzlari_Restoranlar_RestoranId",
                        column: x => x.RestoranId,
                        principalTable: "Restoranlar",
                        principalColumn: "RestoranId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MenuKalemleri",
                columns: table => new
                {
                    MenuKalemiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestoranId = table.Column<int>(type: "int", nullable: false),
                    KategoriId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fiyat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ResimUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HazirlamaSuresi = table.Column<int>(type: "int", nullable: false),
                    IsVejetaryen = table.Column<bool>(type: "bit", nullable: false),
                    IsAkti = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuKalemleri", x => x.MenuKalemiId);
                    table.CheckConstraint("CK_MenuKalemleri_Fiyat", "Fiyat > 0");
                    table.ForeignKey(
                        name: "FK_MenuKalemleri_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "Kategoriler",
                        principalColumn: "KategoriId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuKalemleri_Restoranlar_RestoranId",
                        column: x => x.RestoranId,
                        principalTable: "Restoranlar",
                        principalColumn: "RestoranId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Siparisler",
                columns: table => new
                {
                    SiparisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriKullaniciId = table.Column<int>(type: "int", nullable: false),
                    RestoranId = table.Column<int>(type: "int", nullable: false),
                    KuryeId = table.Column<int>(type: "int", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TeslimatUcreti = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TeslimatAdresi = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SiparisNotu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsAskidaYemek = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeslimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Siparisler", x => x.SiparisId);
                    table.CheckConstraint("CK_Siparisler_Durum", "Durum IN ('Beklemede','Onaylandi','Hazirlaniyor','YoldaKurye','TeslimEdildi','Iptal')");
                    table.CheckConstraint("CK_Siparisler_ToplamTutar", "ToplamTutar > 0");
                    table.ForeignKey(
                        name: "FK_Siparisler_Kullanicilar_MusteriKullaniciId",
                        column: x => x.MusteriKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Siparisler_Kuryeler_KuryeId",
                        column: x => x.KuryeId,
                        principalTable: "Kuryeler",
                        principalColumn: "KuryeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Siparisler_Restoranlar_RestoranId",
                        column: x => x.RestoranId,
                        principalTable: "Restoranlar",
                        principalColumn: "RestoranId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AskidaYemekBagislari",
                columns: table => new
                {
                    BagisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BagisciKullaniciId = table.Column<int>(type: "int", nullable: true),
                    HavuzId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsAnonim = table.Column<bool>(type: "bit", nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BagisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AskidaYemekBagislari", x => x.BagisId);
                    table.CheckConstraint("CK_Bagis_Miktar", "Miktar > 0");
                    table.ForeignKey(
                        name: "FK_AskidaYemekBagislari_AskidaYemekHavuzlari_HavuzId",
                        column: x => x.HavuzId,
                        principalTable: "AskidaYemekHavuzlari",
                        principalColumn: "HavuzId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AskidaYemekBagislari_Kullanicilar_BagisciKullaniciId",
                        column: x => x.BagisciKullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AskidaYemekKullanimlari",
                columns: table => new
                {
                    KullanimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    SiparisId = table.Column<int>(type: "int", nullable: false),
                    HavuzId = table.Column<int>(type: "int", nullable: false),
                    KullanilanMiktar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    KullanimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AskidaYemekKullanimlari", x => x.KullanimId);
                    table.CheckConstraint("CK_Kullanim_Miktar", "KullanilanMiktar > 0");
                    table.ForeignKey(
                        name: "FK_AskidaYemekKullanimlari_AskidaYemekHavuzlari_HavuzId",
                        column: x => x.HavuzId,
                        principalTable: "AskidaYemekHavuzlari",
                        principalColumn: "HavuzId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AskidaYemekKullanimlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AskidaYemekKullanimlari_Siparisler_SiparisId",
                        column: x => x.SiparisId,
                        principalTable: "Siparisler",
                        principalColumn: "SiparisId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiparisDetaylari",
                columns: table => new
                {
                    SiparisDetayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiparisId = table.Column<int>(type: "int", nullable: false),
                    MenuKalemiId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ToplamFiyat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Notlar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiparisDetaylari", x => x.SiparisDetayId);
                    table.CheckConstraint("CK_SiparisDetay_Miktar", "Miktar > 0");
                    table.ForeignKey(
                        name: "FK_SiparisDetaylari_MenuKalemleri_MenuKalemiId",
                        column: x => x.MenuKalemiId,
                        principalTable: "MenuKalemleri",
                        principalColumn: "MenuKalemiId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiparisDetaylari_Siparisler_SiparisId",
                        column: x => x.SiparisId,
                        principalTable: "Siparisler",
                        principalColumn: "SiparisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekBagislari_BagisciKullaniciId",
                table: "AskidaYemekBagislari",
                column: "BagisciKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekBagislari_HavuzId",
                table: "AskidaYemekBagislari",
                column: "HavuzId");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekHavuzlari_RestoranId",
                table: "AskidaYemekHavuzlari",
                column: "RestoranId");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekKullanimlari_HavuzId",
                table: "AskidaYemekKullanimlari",
                column: "HavuzId");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekKullanimlari_KullaniciId",
                table: "AskidaYemekKullanimlari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekKullanimlari_SiparisId",
                table: "AskidaYemekKullanimlari",
                column: "SiparisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Email",
                table: "Kullanicilar",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Telefon",
                table: "Kullanicilar",
                column: "Telefon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kuryeler_KullaniciId",
                table: "Kuryeler",
                column: "KullaniciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuKalemleri_KategoriId",
                table: "MenuKalemleri",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuKalemleri_RestoranId",
                table: "MenuKalemleri",
                column: "RestoranId");

            migrationBuilder.CreateIndex(
                name: "IX_Restoranlar_SahipKullaniciId",
                table: "Restoranlar",
                column: "SahipKullaniciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetaylari_MenuKalemiId",
                table: "SiparisDetaylari",
                column: "MenuKalemiId");

            migrationBuilder.CreateIndex(
                name: "IX_SiparisDetaylari_SiparisId",
                table: "SiparisDetaylari",
                column: "SiparisId");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_KuryeId",
                table: "Siparisler",
                column: "KuryeId");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_MusteriKullaniciId",
                table: "Siparisler",
                column: "MusteriKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Siparisler_RestoranId",
                table: "Siparisler",
                column: "RestoranId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AskidaYemekBagislari");

            migrationBuilder.DropTable(
                name: "AskidaYemekKullanimlari");

            migrationBuilder.DropTable(
                name: "SiparisDetaylari");

            migrationBuilder.DropTable(
                name: "AskidaYemekHavuzlari");

            migrationBuilder.DropTable(
                name: "MenuKalemleri");

            migrationBuilder.DropTable(
                name: "Siparisler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropTable(
                name: "Kuryeler");

            migrationBuilder.DropTable(
                name: "Restoranlar");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YemekSiparisApp.Migrations
{
    /// <inheritdoc />
    public partial class migSipparisDurum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Siparisler_Durum",
                table: "Siparisler");

            migrationBuilder.CreateTable(
                name: "AskidaYemekBasvurulari",
                columns: table => new
                {
                    BasvuruId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BasvuruNotu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdminNotu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BasvuruTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AskidaYemekBasvurulari", x => x.BasvuruId);
                    table.CheckConstraint("CK_Basvuru_Durum", "Durum IN ('Beklemede','Onaylandi','Reddedildi')");
                    table.ForeignKey(
                        name: "FK_AskidaYemekBasvurulari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Siparisler_Durum",
                table: "Siparisler",
                sql: "Durum IN ('Beklemede','Onaylandi','Hazirlaniyor','YoldaKurye','TeslimEdildi','Iptal','AskidaOnayBekliyor')");

            migrationBuilder.CreateIndex(
                name: "IX_AskidaYemekBasvurulari_KullaniciId",
                table: "AskidaYemekBasvurulari",
                column: "KullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AskidaYemekBasvurulari");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Siparisler_Durum",
                table: "Siparisler");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Siparisler_Durum",
                table: "Siparisler",
                sql: "Durum IN ('Beklemede','Onaylandi','Hazirlaniyor','YoldaKurye','TeslimEdildi','Iptal')");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class AddOffresModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EtatsDesOffres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroFeuilleLigne = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateVol = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    PNCVendeurId = table.Column<int>(type: "int", nullable: false),
                    TauxChangeApplique = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ChiffreAffairesEUR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantEncaisseTND = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatsDesOffres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatsDesOffres_PNCs_PNCVendeurId",
                        column: x => x.PNCVendeurId,
                        principalTable: "PNCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtatsDesOffres_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesOffres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesOffresId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotation = table.Column<int>(type: "int", nullable: false),
                    QuantiteCompl = table.Column<int>(type: "int", nullable: false),
                    QuantiteOfferte = table.Column<int>(type: "int", nullable: false),
                    PrixUnitairePromoEUR = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesOffres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesOffres_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesOffres_EtatsDesOffres_EtatDesOffresId",
                        column: x => x.EtatDesOffresId,
                        principalTable: "EtatsDesOffres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesOffres_PNCVendeurId",
                table: "EtatsDesOffres",
                column: "PNCVendeurId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesOffres_VolId",
                table: "EtatsDesOffres",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesOffres_ArticleId",
                table: "LignesOffres",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesOffres_EtatDesOffresId",
                table: "LignesOffres",
                column: "EtatDesOffresId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LignesOffres");

            migrationBuilder.DropTable(
                name: "EtatsDesOffres");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class AddVentesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PNCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricule = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PNCs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroVol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Origine = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtatsDesVentes",
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
                    table.PrimaryKey("PK_EtatsDesVentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatsDesVentes_PNCs_PNCVendeurId",
                        column: x => x.PNCVendeurId,
                        principalTable: "PNCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtatsDesVentes_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesVentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotation = table.Column<int>(type: "int", nullable: false),
                    QuantiteCompl = table.Column<int>(type: "int", nullable: false),
                    QuantiteVendue = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaireEUR = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesVentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesVentes_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesVentes_EtatsDesVentes_EtatDesVentesId",
                        column: x => x.EtatDesVentesId,
                        principalTable: "EtatsDesVentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesVentes_PNCVendeurId",
                table: "EtatsDesVentes",
                column: "PNCVendeurId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesVentes_VolId",
                table: "EtatsDesVentes",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesVentes_ArticleId",
                table: "LignesVentes",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesVentes_EtatDesVentesId",
                table: "LignesVentes",
                column: "EtatDesVentesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LignesVentes");

            migrationBuilder.DropTable(
                name: "EtatsDesVentes");

            migrationBuilder.DropTable(
                name: "PNCs");

            migrationBuilder.DropTable(
                name: "Vols");
        }
    }
}

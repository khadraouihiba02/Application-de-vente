using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class AddFRSTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EtatsDesOffresFRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEtat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutControle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EtatDesOffresId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatsDesOffresFRS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatsDesOffresFRS_EtatsDesOffres_EtatDesOffresId",
                        column: x => x.EtatDesOffresId,
                        principalTable: "EtatsDesOffres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtatsDesVentesFRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEtat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantFRS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatutControle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EtatDesVentesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatsDesVentesFRS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatsDesVentesFRS_EtatsDesVentes_EtatDesVentesId",
                        column: x => x.EtatDesVentesId,
                        principalTable: "EtatsDesVentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesOffresFRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesOffresFRSId = table.Column<int>(type: "int", nullable: false),
                    CodeArticle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NomArticle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DotationInitialeFRS = table.Column<int>(type: "int", nullable: false),
                    QuantiteRestanteFRS = table.Column<int>(type: "int", nullable: false),
                    QuantiteConsommeeFRS = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesOffresFRS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesOffresFRS_EtatsDesOffresFRS_EtatDesOffresFRSId",
                        column: x => x.EtatDesOffresFRSId,
                        principalTable: "EtatsDesOffresFRS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesVentesFRS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesFRSId = table.Column<int>(type: "int", nullable: false),
                    CodeArticle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NomArticle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuantiteVendueFRS = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaireFRS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValeurFRS = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesVentesFRS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesVentesFRS_EtatsDesVentesFRS_EtatDesVentesFRSId",
                        column: x => x.EtatDesVentesFRSId,
                        principalTable: "EtatsDesVentesFRS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesOffresFRS_EtatDesOffresId",
                table: "EtatsDesOffresFRS",
                column: "EtatDesOffresId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesVentesFRS_EtatDesVentesId",
                table: "EtatsDesVentesFRS",
                column: "EtatDesVentesId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesOffresFRS_EtatDesOffresFRSId",
                table: "LignesOffresFRS",
                column: "EtatDesOffresFRSId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesVentesFRS_EtatDesVentesFRSId",
                table: "LignesVentesFRS",
                column: "EtatDesVentesFRSId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LignesOffresFRS");

            migrationBuilder.DropTable(
                name: "LignesVentesFRS");

            migrationBuilder.DropTable(
                name: "EtatsDesOffresFRS");

            migrationBuilder.DropTable(
                name: "EtatsDesVentesFRS");
        }
    }
}

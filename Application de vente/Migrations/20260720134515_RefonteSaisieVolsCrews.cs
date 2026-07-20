using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class RefonteSaisieVolsCrews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EtatsDesOffres_PNCs_PNCVendeurId",
                table: "EtatsDesOffres");

            migrationBuilder.DropForeignKey(
                name: "FK_EtatsDesOffres_Vols_VolId",
                table: "EtatsDesOffres");

            migrationBuilder.DropForeignKey(
                name: "FK_EtatsDesVentes_Vols_VolId",
                table: "EtatsDesVentes");

            migrationBuilder.DropIndex(
                name: "IX_EtatsDesVentes_VolId",
                table: "EtatsDesVentes");

            migrationBuilder.DropIndex(
                name: "IX_EtatsDesOffres_PNCVendeurId",
                table: "EtatsDesOffres");

            migrationBuilder.DropIndex(
                name: "IX_EtatsDesOffres_VolId",
                table: "EtatsDesOffres");

            migrationBuilder.DropColumn(
                name: "VolId",
                table: "EtatsDesVentes");

            migrationBuilder.DropColumn(
                name: "PNCVendeurId",
                table: "EtatsDesOffres");

            migrationBuilder.DropColumn(
                name: "VolId",
                table: "EtatsDesOffres");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVol",
                table: "Vols",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "MontantEncaisseReel",
                table: "EtatsDesVentes",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CrewAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolId = table.Column<int>(type: "int", nullable: false),
                    PNCId = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewAssignments_PNCs_PNCId",
                        column: x => x.PNCId,
                        principalTable: "PNCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewAssignments_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtatDesOffresVols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesOffresId = table.Column<int>(type: "int", nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesOffresVols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatDesOffresVols_EtatsDesOffres_EtatDesOffresId",
                        column: x => x.EtatDesOffresId,
                        principalTable: "EtatsDesOffres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtatDesOffresVols_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EtatDesVentesVols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesId = table.Column<int>(type: "int", nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesVentesVols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatDesVentesVols_EtatsDesVentes_EtatDesVentesId",
                        column: x => x.EtatDesVentesId,
                        principalTable: "EtatsDesVentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtatDesVentesVols_Vols_VolId",
                        column: x => x.VolId,
                        principalTable: "Vols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_PNCId",
                table: "CrewAssignments",
                column: "PNCId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_VolId",
                table: "CrewAssignments",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesOffresVols_EtatDesOffresId",
                table: "EtatDesOffresVols",
                column: "EtatDesOffresId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesOffresVols_VolId",
                table: "EtatDesOffresVols",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesVols_EtatDesVentesId",
                table: "EtatDesVentesVols",
                column: "EtatDesVentesId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesVols_VolId",
                table: "EtatDesVentesVols",
                column: "VolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewAssignments");

            migrationBuilder.DropTable(
                name: "EtatDesOffresVols");

            migrationBuilder.DropTable(
                name: "EtatDesVentesVols");

            migrationBuilder.DropColumn(
                name: "DateVol",
                table: "Vols");

            migrationBuilder.DropColumn(
                name: "MontantEncaisseReel",
                table: "EtatsDesVentes");

            migrationBuilder.AddColumn<int>(
                name: "VolId",
                table: "EtatsDesVentes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PNCVendeurId",
                table: "EtatsDesOffres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VolId",
                table: "EtatsDesOffres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesVentes_VolId",
                table: "EtatsDesVentes",
                column: "VolId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesOffres_PNCVendeurId",
                table: "EtatsDesOffres",
                column: "PNCVendeurId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatsDesOffres_VolId",
                table: "EtatsDesOffres",
                column: "VolId");

            migrationBuilder.AddForeignKey(
                name: "FK_EtatsDesOffres_PNCs_PNCVendeurId",
                table: "EtatsDesOffres",
                column: "PNCVendeurId",
                principalTable: "PNCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EtatsDesOffres_Vols_VolId",
                table: "EtatsDesOffres",
                column: "VolId",
                principalTable: "Vols",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EtatsDesVentes_Vols_VolId",
                table: "EtatsDesVentes",
                column: "VolId",
                principalTable: "Vols",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

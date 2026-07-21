using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class AddCateringModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroFacture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateFacture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Redevances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mois = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    NombrePassagers = table.Column<int>(type: "int", nullable: false),
                    ChiffreAffairesTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantMinGaranti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantPourcentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontantRetenu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MethodeAppliquee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatutFacturation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateCalcul = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redevances", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "Redevances");
        }
    }
}

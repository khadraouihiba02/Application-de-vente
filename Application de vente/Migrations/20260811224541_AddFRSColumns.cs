using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class AddFRSColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChiffreAffairesEUR",
                table: "EtatsDesVentesFRS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantDeclareReelTND",
                table: "EtatsDesVentesFRS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantTheoriqueTND",
                table: "EtatsDesVentesFRS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TauxChangeApplique",
                table: "EtatsDesVentesFRS",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Articles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChiffreAffairesEUR",
                table: "EtatsDesVentesFRS");

            migrationBuilder.DropColumn(
                name: "MontantDeclareReelTND",
                table: "EtatsDesVentesFRS");

            migrationBuilder.DropColumn(
                name: "MontantTheoriqueTND",
                table: "EtatsDesVentesFRS");

            migrationBuilder.DropColumn(
                name: "TauxChangeApplique",
                table: "EtatsDesVentesFRS");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Articles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}

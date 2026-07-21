using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application_de_vente.Migrations
{
    /// <inheritdoc />
    public partial class RefontePNCVol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewAssignments");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "PNCs");

            migrationBuilder.RenameColumn(
                name: "Origine",
                table: "Vols",
                newName: "DEP_AP_ACTUAL");

            migrationBuilder.RenameColumn(
                name: "NumeroVol",
                table: "Vols",
                newName: "FN_NUMBER");

            migrationBuilder.RenameColumn(
                name: "Destination",
                table: "Vols",
                newName: "ARR_AP_ACTUAL");

            migrationBuilder.RenameColumn(
                name: "DateVol",
                table: "Vols",
                newName: "DAY_OF_ORIGIN");

            migrationBuilder.RenameColumn(
                name: "Prenom",
                table: "PNCs",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Nom",
                table: "PNCs",
                newName: "First_name");

            migrationBuilder.RenameColumn(
                name: "Matricule",
                table: "PNCs",
                newName: "TLC");

            migrationBuilder.AddColumn<DateTime>(
                name: "Day_of_origin",
                table: "PNCs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "PNCs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "PNCs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "departure",
                table: "PNCs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "destination",
                table: "PNCs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day_of_origin",
                table: "PNCs");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "PNCs");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "PNCs");

            migrationBuilder.DropColumn(
                name: "departure",
                table: "PNCs");

            migrationBuilder.DropColumn(
                name: "destination",
                table: "PNCs");

            migrationBuilder.RenameColumn(
                name: "FN_NUMBER",
                table: "Vols",
                newName: "NumeroVol");

            migrationBuilder.RenameColumn(
                name: "DEP_AP_ACTUAL",
                table: "Vols",
                newName: "Origine");

            migrationBuilder.RenameColumn(
                name: "DAY_OF_ORIGIN",
                table: "Vols",
                newName: "DateVol");

            migrationBuilder.RenameColumn(
                name: "ARR_AP_ACTUAL",
                table: "Vols",
                newName: "Destination");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "PNCs",
                newName: "Prenom");

            migrationBuilder.RenameColumn(
                name: "TLC",
                table: "PNCs",
                newName: "Matricule");

            migrationBuilder.RenameColumn(
                name: "First_name",
                table: "PNCs",
                newName: "Nom");

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "PNCs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CrewAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PNCId = table.Column<int>(type: "int", nullable: false),
                    VolId = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_PNCId",
                table: "CrewAssignments",
                column: "PNCId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_VolId",
                table: "CrewAssignments",
                column: "VolId");
        }
    }
}

using FamilyApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    [DbContext(typeof(dbContext))]
    [Migration("20260601143000_AddWorkshopBusinessProfile")]
    public partial class AddWorkshopBusinessProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Workshop",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "automotive");

            migrationBuilder.AddColumn<string>(
                name: "TerminologyProfile",
                table: "Workshop",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "automotive");

            migrationBuilder.Sql(@"
UPDATE Workshop
SET BusinessType = 'technical_services',
    TerminologyProfile = 'equipment_service'
WHERE Nif = 'B55667788' OR Nombre LIKE '%ClimaHogar%';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "TerminologyProfile",
                table: "Workshop");
        }
    }
}

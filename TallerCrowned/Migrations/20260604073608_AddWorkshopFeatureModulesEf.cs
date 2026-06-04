using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopFeatureModulesEf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableInvoiceExport",
                table: "Workshop",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableProfitAndLoss",
                table: "Workshop",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWhatsappAlerts",
                table: "Workshop",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableInvoiceExport",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "EnableProfitAndLoss",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "EnableWhatsappAlerts",
                table: "Workshop");
        }
    }
}

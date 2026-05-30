using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddScopedInvoiceNumbering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NumeradorFactura_Anio",
                table: "NumeradorFactura");

            migrationBuilder.AddColumn<string>(
                name: "OwnerKey",
                table: "NumeradorFactura",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: false,
                defaultValue: "legacy");

            migrationBuilder.AddColumn<string>(
                name: "Serie",
                table: "NumeradorFactura",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "A");

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_OwnerKey_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "OwnerKey", "Serie", "Anio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NumeradorFactura_OwnerKey_Serie_Anio",
                table: "NumeradorFactura");

            migrationBuilder.DropColumn(
                name: "OwnerKey",
                table: "NumeradorFactura");

            migrationBuilder.DropColumn(
                name: "Serie",
                table: "NumeradorFactura");

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_Anio",
                table: "NumeradorFactura",
                column: "Anio",
                unique: true);
        }
    }
}

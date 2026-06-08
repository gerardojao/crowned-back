using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCantidadToOrdenTrabajoAndPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cantidad",
                table: "Presupuesto",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "Cantidad",
                table: "OrdenTrabajo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 1m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Presupuesto");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "OrdenTrabajo");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderEstimatedTimeAndDeliveryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrevistaEntrega",
                table: "PreOrdenTrabajo",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoEstimadoHoras",
                table: "PreOrdenTrabajo",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrevistaEntrega",
                table: "OrdenTrabajo",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TiempoEstimadoHoras",
                table: "OrdenTrabajo",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaPrevistaEntrega",
                table: "PreOrdenTrabajo");

            migrationBuilder.DropColumn(
                name: "TiempoEstimadoHoras",
                table: "PreOrdenTrabajo");

            migrationBuilder.DropColumn(
                name: "FechaPrevistaEntrega",
                table: "OrdenTrabajo");

            migrationBuilder.DropColumn(
                name: "TiempoEstimadoHoras",
                table: "OrdenTrabajo");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaRectificativaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacturaOriginalId",
                table: "FacturaEmitida",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRectificacion",
                table: "FacturaEmitida",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImporteRectificado",
                table: "FacturaEmitida",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRectificacion",
                table: "FacturaEmitida",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroFacturaRectificada",
                table: "FacturaEmitida",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoFactura",
                table: "FacturaEmitida",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_FacturaOriginalId",
                table: "FacturaEmitida",
                column: "FacturaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_TipoFactura",
                table: "FacturaEmitida",
                column: "TipoFactura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaEmitida_FacturaOriginalId",
                table: "FacturaEmitida");

            migrationBuilder.DropIndex(
                name: "IX_FacturaEmitida_TipoFactura",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "FacturaOriginalId",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "FechaRectificacion",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "ImporteRectificado",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "MotivoRectificacion",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "NumeroFacturaRectificada",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "TipoFactura",
                table: "FacturaEmitida");
        }
    }
}

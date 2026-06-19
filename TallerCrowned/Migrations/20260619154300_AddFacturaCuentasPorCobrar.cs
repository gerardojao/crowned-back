using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaCuentasPorCobrar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoCxC",
                table: "FacturaEmitida",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Pagada");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "FacturaEmitida",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPendiente",
                table: "FacturaEmitida",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TipoPago",
                table: "FacturaEmitida",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Contado");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAbonado",
                table: "FacturaEmitida",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFactura",
                table: "FacturaEmitida",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE [FacturaEmitida]
                SET
                    [TotalFactura] = [Total],
                    [TotalAbonado] = [Total],
                    [SaldoPendiente] = 0,
                    [TipoPago] = 'Contado',
                    [EstadoCxC] = 'Pagada'
                WHERE [TotalFactura] = 0 AND [Total] > 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_EstadoCxC",
                table: "FacturaEmitida",
                column: "EstadoCxC");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_FechaVencimiento",
                table: "FacturaEmitida",
                column: "FechaVencimiento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturaEmitida_EstadoCxC",
                table: "FacturaEmitida");

            migrationBuilder.DropIndex(
                name: "IX_FacturaEmitida_FechaVencimiento",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "EstadoCxC",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "SaldoPendiente",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "TipoPago",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "TotalAbonado",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "TotalFactura",
                table: "FacturaEmitida");
        }
    }
}

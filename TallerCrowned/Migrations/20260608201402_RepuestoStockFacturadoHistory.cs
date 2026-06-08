using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class RepuestoStockFacturadoHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdProveedor",
                table: "RepuestoStock",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "RepuestoStock",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "RepuestoStock",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsFacturado",
                table: "RepuestoStock",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFactura",
                table: "RepuestoStock",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdFacturaEmitida",
                table: "RepuestoStock",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "RepuestoStock",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreProveedorSnapshot",
                table: "RepuestoStock",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroFactura",
                table: "RepuestoStock",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_EsFacturado",
                table: "RepuestoStock",
                column: "EsFacturado");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_FechaFactura",
                table: "RepuestoStock",
                column: "FechaFactura");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_NumeroFactura",
                table: "RepuestoStock",
                column: "NumeroFactura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepuestoStock_EsFacturado",
                table: "RepuestoStock");

            migrationBuilder.DropIndex(
                name: "IX_RepuestoStock_FechaFactura",
                table: "RepuestoStock");

            migrationBuilder.DropIndex(
                name: "IX_RepuestoStock_NumeroFactura",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "EsFacturado",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "FechaFactura",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "IdFacturaEmitida",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "NombreProveedorSnapshot",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "NumeroFactura",
                table: "RepuestoStock");

            migrationBuilder.AlterColumn<int>(
                name: "IdProveedor",
                table: "RepuestoStock",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Cantidad",
                table: "RepuestoStock",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}

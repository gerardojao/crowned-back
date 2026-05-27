using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRepuestoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepuestoStock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    CodigoReferencia = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Marca = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Categoria = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    PrecioCompra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ubicacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestoStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepuestoStock_Proveedor_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_Categoria",
                table: "RepuestoStock",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_CodigoReferencia",
                table: "RepuestoStock",
                column: "CodigoReferencia");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_IdProveedor",
                table: "RepuestoStock",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_Nombre",
                table: "RepuestoStock",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepuestoStock");
        }
    }
}

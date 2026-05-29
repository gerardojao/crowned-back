using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaEmitida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacturaEmitida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroFactura = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    IdOrdenTrabajo = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cliente = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Dni = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    DireccionCliente = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    TelefonoCliente = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Km = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Iva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Otros = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaEmitida", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_IdOrdenTrabajo",
                table: "FacturaEmitida",
                column: "IdOrdenTrabajo");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_NumeroFactura",
                table: "FacturaEmitida",
                column: "NumeroFactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_UsuarioCreacion_Eliminado",
                table: "FacturaEmitida",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaEmitida");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableLedger",
                table: "Workshop",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "MayorMovimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cuenta = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TipoMovimiento = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WorkshopId = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MayorMovimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MayorMovimiento_Workshop_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MayorMovimiento_Cuenta",
                table: "MayorMovimiento",
                column: "Cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_MayorMovimiento_Fecha",
                table: "MayorMovimiento",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MayorMovimiento_TipoMovimiento",
                table: "MayorMovimiento",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MayorMovimiento_UsuarioCreacion_Eliminado",
                table: "MayorMovimiento",
                columns: new[] { "UsuarioCreacion", "Eliminado" });

            migrationBuilder.CreateIndex(
                name: "IX_MayorMovimiento_WorkshopId",
                table: "MayorMovimiento",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MayorMovimiento");

            migrationBuilder.DropColumn(
                name: "EnableLedger",
                table: "Workshop");
        }
    }
}

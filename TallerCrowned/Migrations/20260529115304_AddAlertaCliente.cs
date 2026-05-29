using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertaCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cliente = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Mensaje = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    FechaAviso = table.Column<DateTime>(type: "datetime", nullable: false),
                    Atendida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IdFacturaEmitida = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AlertaCliente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertaCliente_Atendida",
                table: "AlertaCliente",
                column: "Atendida");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaCliente_FechaAviso",
                table: "AlertaCliente",
                column: "FechaAviso");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaCliente_UsuarioCreacion_Eliminado",
                table: "AlertaCliente",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertaCliente");
        }
    }
}

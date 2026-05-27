using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Contacto = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    Categoria = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    NifCif = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Categoria",
                table: "Proveedor",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Nombre",
                table: "Proveedor",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_UsuarioCreacion_Eliminado",
                table: "Proveedor",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proveedor");
        }
    }
}

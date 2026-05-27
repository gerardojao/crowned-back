using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Modelo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Matricula",
                table: "Cliente",
                column: "Matricula");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Telefono",
                table: "Cliente",
                column: "Telefono");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_UsuarioCreacion_Eliminado",
                table: "Cliente",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cliente");
        }
    }
}

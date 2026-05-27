using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdenTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cliente = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Modelo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    Trabajo = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Repuestos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManoObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenTrabajo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_Matricula",
                table: "OrdenTrabajo",
                column: "Matricula");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_UsuarioCreacion_Eliminado_Fecha",
                table: "OrdenTrabajo",
                columns: new[] { "UsuarioCreacion", "Eliminado", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenTrabajo");
        }
    }
}

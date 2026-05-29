using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Presupuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroPresupuesto = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Cliente = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Modelo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Trabajo = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    Repuestos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManoObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Pendiente"),
                    Observaciones = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    ConvertidoEnOrden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IdOrdenTrabajo = table.Column<int>(type: "int", nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuesto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_Estado",
                table: "Presupuesto",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_Matricula",
                table: "Presupuesto",
                column: "Matricula");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_NumeroPresupuesto",
                table: "Presupuesto",
                column: "NumeroPresupuesto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_UsuarioCreacion_Eliminado",
                table: "Presupuesto",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Presupuesto");
        }
    }
}

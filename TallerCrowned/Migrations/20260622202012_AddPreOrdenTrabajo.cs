using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPreOrdenTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreOrdenTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cliente = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Dni = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Matricula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Modelo = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    MotivoRecepcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    DiagnosticoMecanico = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    RepuestosNecesarios = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ConvertidaEnOrden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IdOrdenTrabajo = table.Column<int>(type: "int", nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WorkshopId = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreOrdenTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreOrdenTrabajo_Workshop_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreOrdenTrabajo_Matricula",
                table: "PreOrdenTrabajo",
                column: "Matricula");

            migrationBuilder.CreateIndex(
                name: "IX_PreOrdenTrabajo_UsuarioCreacion_Eliminado_Fecha",
                table: "PreOrdenTrabajo",
                columns: new[] { "UsuarioCreacion", "Eliminado", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_PreOrdenTrabajo_WorkshopId",
                table: "PreOrdenTrabajo",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreOrdenTrabajo");
        }
    }
}

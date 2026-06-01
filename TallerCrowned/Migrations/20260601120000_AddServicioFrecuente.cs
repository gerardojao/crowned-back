using System;
using FamilyApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    [DbContext(typeof(dbContext))]
    [Migration("20260601120000_AddServicioFrecuente")]
    public partial class AddServicioFrecuente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicioFrecuente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(180)", unicode: false, maxLength: 180, nullable: false),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicioFrecuente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicioFrecuente_Workshop_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicioFrecuente_UsuarioCreacion_Eliminado",
                table: "ServicioFrecuente",
                columns: new[] { "UsuarioCreacion", "Eliminado" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicioFrecuente_WorkshopId",
                table: "ServicioFrecuente",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicioFrecuente_WorkshopId_Nombre",
                table: "ServicioFrecuente",
                columns: new[] { "WorkshopId", "Nombre" },
                unique: true);

            migrationBuilder.Sql(@"
DECLARE @now datetime2 = SYSUTCDATETIME();

INSERT INTO [ServicioFrecuente] ([Nombre], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
SELECT v.[Nombre], 0, 1, 'seed:servicios', @now, 'seed:servicios', @now, w.[Id]
FROM [Workshop] w
CROSS JOIN (VALUES
    ('Servicio cambio de aceite y filtro'),
    ('Cambio de pastillas de frenos'),
    ('Cambio de rodamientos delanteros'),
    ('Cambio de amortiguadores'),
    ('Mano de obra'),
    ('Repuestos')
) v([Nombre])
WHERE NOT EXISTS (
    SELECT 1
    FROM [ServicioFrecuente] sf
    WHERE sf.[WorkshopId] = w.[Id]
      AND LOWER(sf.[Nombre]) = LOWER(v.[Nombre])
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicioFrecuente");
        }
    }
}

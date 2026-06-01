using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopTenantPhase4A : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "RepuestoStock",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Proveedor",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Presupuesto",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "OrdenTrabajo",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "NumeradorFactura",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Ingreso",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "FichaIngreso",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "FichaEgreso",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "FacturaEmitida",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Egresos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "Cliente",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "WorkshopId",
                table: "AlertaCliente",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Workshop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    RazonSocial = table.Column<string>(type: "varchar(180)", unicode: false, maxLength: 180, nullable: false),
                    Nif = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Direccion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Iban = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SerieFactura = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "A"),
                    LogoPath = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workshop", x => x.Id);
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Workshop] ON;
IF NOT EXISTS (SELECT 1 FROM [Workshop] WHERE [Id] = 1)
BEGIN
    INSERT INTO [Workshop]
        ([Id], [Nombre], [RazonSocial], [Nif], [Direccion], [Telefono], [Email], [Iban], [SerieFactura], [LogoPath], [Activo], [FechaCreacion])
    VALUES
        (1, 'Multiservicios Crower', 'JUAN CARLOS FERNANDEZ SILVA', '61407055E', 'CALLE ALCACER 63 D, Albal, 46470', '960057935/655042253', 'multiservicioscrower@gmail.com', 'ES69 2100 4014 9122 0012 3843', 'A', '/uploads/workshops/LogoTallerCrowned.png', 1, GETUTCDATE());
END
SET IDENTITY_INSERT [Workshop] OFF;
");

            migrationBuilder.CreateTable(
                name: "WorkshopUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "owner"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopUser_Workshop_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
SELECT 1, [Id], 'owner', 1, GETUTCDATE()
FROM [Users] u
WHERE NOT EXISTS (
    SELECT 1
    FROM [WorkshopUser] wu
    WHERE wu.[WorkshopId] = 1 AND wu.[UserId] = u.[Id]
);
");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_WorkshopId",
                table: "RepuestoStock",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_WorkshopId",
                table: "Proveedor",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_WorkshopId",
                table: "Presupuesto",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_WorkshopId",
                table: "OrdenTrabajo",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_WorkshopId_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "WorkshopId", "Serie", "Anio" });

            migrationBuilder.CreateIndex(
                name: "IX_Ingreso_WorkshopId",
                table: "Ingreso",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaIngreso_WorkshopId",
                table: "FichaIngreso",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_FichaEgreso_WorkshopId",
                table: "FichaEgreso",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaEmitida_WorkshopId",
                table: "FacturaEmitida",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_WorkshopId",
                table: "Egresos",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_WorkshopId",
                table: "Cliente",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaCliente_WorkshopId",
                table: "AlertaCliente",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshop_Activo",
                table: "Workshop",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_Workshop_Nif",
                table: "Workshop",
                column: "Nif");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopUser_UserId_Activo",
                table: "WorkshopUser",
                columns: new[] { "UserId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopUser_WorkshopId_UserId",
                table: "WorkshopUser",
                columns: new[] { "WorkshopId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertaCliente_Workshop_WorkshopId",
                table: "AlertaCliente",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Workshop_WorkshopId",
                table: "Cliente",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Workshop_WorkshopId",
                table: "Egresos",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FacturaEmitida_Workshop_WorkshopId",
                table: "FacturaEmitida",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FichaEgreso_Workshop_WorkshopId",
                table: "FichaEgreso",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FichaIngreso_Workshop_WorkshopId",
                table: "FichaIngreso",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingreso_Workshop_WorkshopId",
                table: "Ingreso",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NumeradorFactura_Workshop_WorkshopId",
                table: "NumeradorFactura",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenTrabajo_Workshop_WorkshopId",
                table: "OrdenTrabajo",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuesto_Workshop_WorkshopId",
                table: "Presupuesto",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proveedor_Workshop_WorkshopId",
                table: "Proveedor",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepuestoStock_Workshop_WorkshopId",
                table: "RepuestoStock",
                column: "WorkshopId",
                principalTable: "Workshop",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertaCliente_Workshop_WorkshopId",
                table: "AlertaCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Workshop_WorkshopId",
                table: "Cliente");

            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Workshop_WorkshopId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturaEmitida_Workshop_WorkshopId",
                table: "FacturaEmitida");

            migrationBuilder.DropForeignKey(
                name: "FK_FichaEgreso_Workshop_WorkshopId",
                table: "FichaEgreso");

            migrationBuilder.DropForeignKey(
                name: "FK_FichaIngreso_Workshop_WorkshopId",
                table: "FichaIngreso");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingreso_Workshop_WorkshopId",
                table: "Ingreso");

            migrationBuilder.DropForeignKey(
                name: "FK_NumeradorFactura_Workshop_WorkshopId",
                table: "NumeradorFactura");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenTrabajo_Workshop_WorkshopId",
                table: "OrdenTrabajo");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuesto_Workshop_WorkshopId",
                table: "Presupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_Proveedor_Workshop_WorkshopId",
                table: "Proveedor");

            migrationBuilder.DropForeignKey(
                name: "FK_RepuestoStock_Workshop_WorkshopId",
                table: "RepuestoStock");

            migrationBuilder.DropTable(
                name: "WorkshopUser");

            migrationBuilder.DropTable(
                name: "Workshop");

            migrationBuilder.DropIndex(
                name: "IX_RepuestoStock_WorkshopId",
                table: "RepuestoStock");

            migrationBuilder.DropIndex(
                name: "IX_Proveedor_WorkshopId",
                table: "Proveedor");

            migrationBuilder.DropIndex(
                name: "IX_Presupuesto_WorkshopId",
                table: "Presupuesto");

            migrationBuilder.DropIndex(
                name: "IX_OrdenTrabajo_WorkshopId",
                table: "OrdenTrabajo");

            migrationBuilder.DropIndex(
                name: "IX_NumeradorFactura_WorkshopId_Serie_Anio",
                table: "NumeradorFactura");

            migrationBuilder.DropIndex(
                name: "IX_Ingreso_WorkshopId",
                table: "Ingreso");

            migrationBuilder.DropIndex(
                name: "IX_FichaIngreso_WorkshopId",
                table: "FichaIngreso");

            migrationBuilder.DropIndex(
                name: "IX_FichaEgreso_WorkshopId",
                table: "FichaEgreso");

            migrationBuilder.DropIndex(
                name: "IX_FacturaEmitida_WorkshopId",
                table: "FacturaEmitida");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_WorkshopId",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_WorkshopId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_AlertaCliente_WorkshopId",
                table: "AlertaCliente");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Presupuesto");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "OrdenTrabajo");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "NumeradorFactura");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Ingreso");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "FichaIngreso");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "WorkshopId",
                table: "AlertaCliente");
        }
    }
}

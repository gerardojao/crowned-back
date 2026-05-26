using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    public partial class AuditableAndUserOwnerShadowProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- FichaEgreso ----
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "FichaEgreso",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "FichaEgreso",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "FichaEgreso",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FichaEgreso",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreacion",
                table: "FichaEgreso",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioModificacion",
                table: "FichaEgreso",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FichaEgreso_UserId_Eliminado_Fecha",
                table: "FichaEgreso",
                columns: new[] { "UserId", "Eliminado", "Fecha" });

            // ---- FichaIngreso ----
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "FichaIngreso",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "FichaIngreso",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "FichaIngreso",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FichaIngreso",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreacion",
                table: "FichaIngreso",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioModificacion",
                table: "FichaIngreso",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FichaIngreso_UserId_Eliminado_Fecha",
                table: "FichaIngreso",
                columns: new[] { "UserId", "Eliminado", "Fecha" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FichaEgreso_UserId_Eliminado_Fecha",
                table: "FichaEgreso");

            migrationBuilder.DropIndex(
                name: "IX_FichaIngreso_UserId_Eliminado_Fecha",
                table: "FichaIngreso");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "FichaEgreso");
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "FichaEgreso");
            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "FichaEgreso");
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FichaEgreso");
            migrationBuilder.DropColumn(
                name: "UsuarioCreacion",
                table: "FichaEgreso");
            migrationBuilder.DropColumn(
                name: "UsuarioModificacion",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "FichaIngreso");
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "FichaIngreso");
            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "FichaIngreso");
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FichaIngreso");
            migrationBuilder.DropColumn(
                name: "UsuarioCreacion",
                table: "FichaIngreso");
            migrationBuilder.DropColumn(
                name: "UsuarioModificacion",
                table: "FichaIngreso");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToRepuestoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "RepuestoStock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "RepuestoStock",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCreacion",
                table: "RepuestoStock",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioModificacion",
                table: "RepuestoStock",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepuestoStock_UsuarioCreacion_Eliminado",
                table: "RepuestoStock",
                columns: new[] { "UsuarioCreacion", "Eliminado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RepuestoStock_UsuarioCreacion_Eliminado",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "UsuarioCreacion",
                table: "RepuestoStock");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacion",
                table: "RepuestoStock");
        }
    }
}

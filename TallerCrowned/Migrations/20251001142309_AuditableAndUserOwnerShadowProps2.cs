using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AuditableAndUserOwnerShadowProps2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Eliminado",
                table: "FichaIngreso",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FichaIngreso",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FichaEgreso",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FichaIngreso_UserId_Eliminado_Fecha",
                table: "FichaIngreso",
                columns: new[] { "UserId", "Eliminado", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_FichaEgreso_UserId_Eliminado_Fecha",
                table: "FichaEgreso",
                columns: new[] { "UserId", "Eliminado", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FichaIngreso_UserId_Eliminado_Fecha",
                table: "FichaIngreso");

            migrationBuilder.DropIndex(
                name: "IX_FichaEgreso_UserId_Eliminado_Fecha",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FichaIngreso");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FichaEgreso");

            migrationBuilder.AlterColumn<bool>(
                name: "Eliminado",
                table: "FichaIngreso",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}

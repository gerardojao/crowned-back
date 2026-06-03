using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    public partial class AddEgresoTipoGasto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Egresos', 'TipoGasto') IS NULL
                    ALTER TABLE [Egresos] ADD [TipoGasto] varchar(20) NOT NULL CONSTRAINT [DF_Egresos_TipoGasto] DEFAULT 'variable';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Egresos', 'TipoGasto') IS NOT NULL
                    ALTER TABLE [Egresos] DROP COLUMN [TipoGasto];
                """);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class ActivateWorkshopTenantPhase4B : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_NumeradorFactura_OwnerKey_Serie_Anio'
                        AND object_id = OBJECT_ID('NumeradorFactura')
                )
                    DROP INDEX IX_NumeradorFactura_OwnerKey_Serie_Anio ON NumeradorFactura;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_NumeradorFactura_WorkshopId_Serie_Anio'
                        AND object_id = OBJECT_ID('NumeradorFactura')
                )
                    DROP INDEX IX_NumeradorFactura_WorkshopId_Serie_Anio ON NumeradorFactura;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_OwnerKey_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "OwnerKey", "Serie", "Anio" });

            migrationBuilder.Sql("""
                ;WITH RankedNumeradores AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (
                            PARTITION BY WorkshopId, Serie, Anio
                            ORDER BY UltimoNumero DESC, Id ASC
                        ) AS RowNumber
                    FROM NumeradorFactura
                )
                DELETE FROM NumeradorFactura
                WHERE Id IN (
                    SELECT Id
                    FROM RankedNumeradores
                    WHERE RowNumber > 1
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_WorkshopId_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "WorkshopId", "Serie", "Anio" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_NumeradorFactura_OwnerKey_Serie_Anio'
                        AND object_id = OBJECT_ID('NumeradorFactura')
                )
                    DROP INDEX IX_NumeradorFactura_OwnerKey_Serie_Anio ON NumeradorFactura;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_NumeradorFactura_WorkshopId_Serie_Anio'
                        AND object_id = OBJECT_ID('NumeradorFactura')
                )
                    DROP INDEX IX_NumeradorFactura_WorkshopId_Serie_Anio ON NumeradorFactura;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_OwnerKey_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "OwnerKey", "Serie", "Anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumeradorFactura_WorkshopId_Serie_Anio",
                table: "NumeradorFactura",
                columns: new[] { "WorkshopId", "Serie", "Anio" });
        }
    }
}

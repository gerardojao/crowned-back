using FamilyApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    [DbContext(typeof(dbContext))]
    [Migration("20260531170000_ScopeBudgetNumberByWorkshop")]
    public partial class ScopeBudgetNumberByWorkshop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Presupuesto_NumeroPresupuesto",
                table: "Presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_WorkshopId_NumeroPresupuesto",
                table: "Presupuesto",
                columns: new[] { "WorkshopId", "NumeroPresupuesto" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Presupuesto_WorkshopId_NumeroPresupuesto",
                table: "Presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuesto_NumeroPresupuesto",
                table: "Presupuesto",
                column: "NumeroPresupuesto",
                unique: true);
        }
    }
}

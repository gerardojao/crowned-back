using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountIban",
                table: "FichaEgreso",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "FichaEgreso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "FichaEgreso",
                type: "varchar(120)",
                unicode: false,
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountIban",
                table: "FacturaEmitida",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "FacturaEmitida",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "FacturaEmitida",
                type: "varchar(120)",
                unicode: false,
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkshopBankAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(120)", unicode: false, maxLength: 120, nullable: false),
                    Iban = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopBankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopBankAccount_Workshop_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO WorkshopBankAccount (WorkshopId, Nombre, Iban, EsPrincipal, Activo, FechaCreacion)
                SELECT Id, 'Cuenta principal', Iban, 1, 1, GETUTCDATE()
                FROM Workshop
                WHERE Iban IS NOT NULL
                  AND LTRIM(RTRIM(Iban)) <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM WorkshopBankAccount b
                      WHERE b.WorkshopId = Workshop.Id
                  );

                UPDATE f
                SET
                    f.BankAccountId = b.Id,
                    f.BankAccountName = b.Nombre,
                    f.BankAccountIban = b.Iban
                FROM FacturaEmitida f
                INNER JOIN WorkshopBankAccount b
                    ON b.WorkshopId = f.WorkshopId
                   AND b.EsPrincipal = 1
                   AND b.Activo = 1
                WHERE f.BankAccountId IS NULL;

                UPDATE e
                SET
                    e.BankAccountId = b.Id,
                    e.BankAccountName = b.Nombre,
                    e.BankAccountIban = b.Iban
                FROM FichaEgreso e
                INNER JOIN WorkshopBankAccount b
                    ON b.WorkshopId = e.WorkshopId
                   AND b.EsPrincipal = 1
                   AND b.Activo = 1
                WHERE e.BankAccountId IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopBankAccount_WorkshopId_Activo",
                table: "WorkshopBankAccount",
                columns: new[] { "WorkshopId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopBankAccount_WorkshopId_Iban",
                table: "WorkshopBankAccount",
                columns: new[] { "WorkshopId", "Iban" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkshopBankAccount");

            migrationBuilder.DropColumn(
                name: "BankAccountIban",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "FichaEgreso");

            migrationBuilder.DropColumn(
                name: "BankAccountIban",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "FacturaEmitida");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "FacturaEmitida");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountToFichaIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountIban",
                table: "FichaIngreso",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "FichaIngreso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "FichaIngreso",
                type: "varchar(120)",
                unicode: false,
                maxLength: 120,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE i
                SET
                    i.BankAccountId = b.Id,
                    i.BankAccountName = b.Nombre,
                    i.BankAccountIban = b.Iban
                FROM FichaIngreso i
                INNER JOIN WorkshopBankAccount b
                    ON b.WorkshopId = i.WorkshopId
                   AND b.EsPrincipal = 1
                   AND b.Activo = 1
                WHERE i.BankAccountId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountIban",
                table: "FichaIngreso");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "FichaIngreso");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "FichaIngreso");
        }
    }
}

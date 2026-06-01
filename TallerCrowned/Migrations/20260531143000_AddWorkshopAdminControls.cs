using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FamilyApp.Data;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(dbContext))]
    [Migration("20260531143000_AddWorkshopAdminControls")]
    public partial class AddWorkshopAdminControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "Workshop",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                table: "Workshop",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyText",
                table: "Workshop",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsText",
                table: "Workshop",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Workshop]
                SET [MaxUsers] = 3
                WHERE [MaxUsers] IS NULL OR [MaxUsers] <= 0;

                UPDATE [Workshop]
                SET [FooterText] = CONCAT(N'© ', YEAR(GETDATE()), N' ', [Nombre], N'. Todos los derechos reservados.')
                WHERE [FooterText] IS NULL;

                UPDATE [Users]
                SET [Role] = 'superadmin'
                WHERE [Email] = 'gerardojao@gmail.com';

                UPDATE [Users]
                SET [Role] = 'admin'
                WHERE [Email] = 'admin@familyapp.io' AND [Role] = 'superadmin';

                DECLARE @zagaWorkshopId int;
                SELECT @zagaWorkshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B10987654';

                IF @zagaWorkshopId IS NOT NULL
                BEGIN
                    UPDATE wu
                    SET [Activo] = 0
                    FROM [WorkshopUser] wu
                    INNER JOIN [Users] u ON u.[Id] = wu.[UserId]
                    WHERE wu.[WorkshopId] = @zagaWorkshopId
                      AND u.[Email] = 'zaga.viewer@tallercrowned.test';
                END

                DECLARE @crowerWorkshopId int;
                SELECT @crowerWorkshopId = [Id] FROM [Workshop] WHERE [Nif] = '61407055E';

                IF @crowerWorkshopId IS NOT NULL
                BEGIN
                    UPDATE wu
                    SET [Activo] = 0
                    FROM [WorkshopUser] wu
                    INNER JOIN [Users] u ON u.[Id] = wu.[UserId]
                    WHERE wu.[WorkshopId] = @crowerWorkshopId
                      AND u.[Email] IN (
                          'admin@familyapp.io',
                          'cliente@demo.com',
                          'ayaconsultorestributarios@gmail.com'
                      );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "FooterText",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyText",
                table: "Workshop");

            migrationBuilder.DropColumn(
                name: "TermsText",
                table: "Workshop");

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [Role] = 'admin'
                WHERE [Email] = 'admin@familyapp.io' AND [Role] = 'superadmin';

                UPDATE [Users]
                SET [Role] = 'user'
                WHERE [Email] = 'gerardojao@gmail.com' AND [Role] = 'superadmin';

                DECLARE @zagaWorkshopId int;
                SELECT @zagaWorkshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B10987654';

                IF @zagaWorkshopId IS NOT NULL
                BEGIN
                    UPDATE wu
                    SET [Activo] = 1
                    FROM [WorkshopUser] wu
                    INNER JOIN [Users] u ON u.[Id] = wu.[UserId]
                    WHERE wu.[WorkshopId] = @zagaWorkshopId
                      AND u.[Email] = 'zaga.viewer@tallercrowned.test';
                END

                DECLARE @crowerWorkshopId int;
                SELECT @crowerWorkshopId = [Id] FROM [Workshop] WHERE [Nif] = '61407055E';

                IF @crowerWorkshopId IS NOT NULL
                BEGIN
                    UPDATE wu
                    SET [Activo] = 1
                    FROM [WorkshopUser] wu
                    INNER JOIN [Users] u ON u.[Id] = wu.[UserId]
                    WHERE wu.[WorkshopId] = @crowerWorkshopId
                      AND u.[Email] IN (
                          'admin@familyapp.io',
                          'cliente@demo.com',
                          'ayaconsultorestributarios@gmail.com'
                      );
                END
                """);
        }
    }
}

using FamilyApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    [DbContext(typeof(dbContext))]
    [Migration("20260601133000_AddClimaHogarDemoTenant")]
    public partial class AddClimaHogarDemoTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET NOCOUNT ON;

                DECLARE @now datetime = GETUTCDATE();
                DECLARE @workshopId int;
                DECLARE @ownerId int;
                DECLARE @managerId int;
                DECLARE @techId int;
                DECLARE @providerId int;

                IF NOT EXISTS (SELECT 1 FROM [Workshop] WHERE [Nif] = 'B55667788')
                BEGIN
                    INSERT INTO [Workshop]
                        ([Nombre], [RazonSocial], [Nif], [Direccion], [Telefono], [Email], [Iban], [SerieFactura], [LogoPath], [MaxUsers], [FooterText], [PrivacyPolicyText], [TermsText], [Activo], [FechaCreacion])
                    VALUES
                        ('ClimaHogar Pro', 'ClimaHogar Pro Servicios Tecnicos S.L.', 'B55667788', 'Calle Energia 18, 28037 Madrid', '910 884 221', 'admin@climahogarpro.test', 'ES9121000418450200051332', 'CH', NULL, 3, '© ClimaHogar Pro. Servicios tecnicos de climatizacion y mantenimiento.', 'Politica de privacidad demo para servicios tecnicos a domicilio.', 'Terminos demo para servicios tecnicos a domicilio.', 1, @now);
                END

                SELECT @workshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B55667788';

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'climahogar.owner@zagapro.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('climahogar.owner@zagapro.test', 'AQAAAAIAAYagAAAAEFsvVjsgJ8P23Wa24gVJd8DsbOBO3qiS6hj5jO2FiWupCg+OaUMPcxY5Qsx8fQN5gw==', 'user', 'ClimaHogar Owner Demo', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'climahogar.manager@zagapro.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('climahogar.manager@zagapro.test', 'AQAAAAIAAYagAAAAEFsvVjsgJ8P23Wa24gVJd8DsbOBO3qiS6hj5jO2FiWupCg+OaUMPcxY5Qsx8fQN5gw==', 'user', 'ClimaHogar Manager Demo', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'climahogar.tech@zagapro.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('climahogar.tech@zagapro.test', 'AQAAAAIAAYagAAAAEFsvVjsgJ8P23Wa24gVJd8DsbOBO3qiS6hj5jO2FiWupCg+OaUMPcxY5Qsx8fQN5gw==', 'user', 'ClimaHogar Tecnico Demo', 1, @now);

                SELECT @ownerId = [Id] FROM [Users] WHERE [Email] = 'climahogar.owner@zagapro.test';
                SELECT @managerId = [Id] FROM [Users] WHERE [Email] = 'climahogar.manager@zagapro.test';
                SELECT @techId = [Id] FROM [Users] WHERE [Email] = 'climahogar.tech@zagapro.test';

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @ownerId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @ownerId, 'owner', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @managerId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @managerId, 'manager', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @techId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @techId, 'mechanic', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [NumeradorFactura] WHERE [WorkshopId] = @workshopId AND [Serie] = 'CH' AND [Anio] = 2026)
                    INSERT INTO [NumeradorFactura] ([WorkshopId], [OwnerKey], [Serie], [Anio], [UltimoNumero])
                    VALUES (@workshopId, CONCAT('workshop:', @workshopId), 'CH', 2026, 0);

                IF NOT EXISTS (SELECT 1 FROM [Proveedor] WHERE [WorkshopId] = @workshopId AND [Nombre] = 'ClimaMarket Suministros')
                    INSERT INTO [Proveedor]
                        ([Nombre], [Contacto], [Telefono], [Email], [Direccion], [Categoria], [NifCif], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('ClimaMarket Suministros', 'Nuria Lopez', '913 220 144', 'pedidos@climamarket.test', 'Poligono Norte, Nave 14', 'Climatizacion', 'B82114490', 'Proveedor demo de materiales de climatizacion.', 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                SELECT @providerId = [Id] FROM [Proveedor] WHERE [WorkshopId] = @workshopId AND [Nombre] = 'ClimaMarket Suministros';

                IF NOT EXISTS (SELECT 1 FROM [Cliente] WHERE [WorkshopId] = @workshopId AND [Matricula] = 'CH-AC-001')
                    INSERT INTO [Cliente]
                        ([Nombre], [Telefono], [Email], [Direccion], [Matricula], [Marca], [Modelo], [Kilometraje], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Laura Medina Ruiz', '634 210 558', 'laura.medina@example.test', 'Calle Ibiza 22, 3B, Madrid', 'CH-AC-001', 'Daikin', 'Split Sensira 3000 frigorias', 4, 'Equipo salon. Cliente demo ClimaHogar.', 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [Cliente] WHERE [WorkshopId] = @workshopId AND [Matricula] = 'CH-TE-014')
                    INSERT INTO [Cliente]
                        ([Nombre], [Telefono], [Email], [Direccion], [Matricula], [Marca], [Modelo], [Kilometraje], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Roberto Cano Gil', '622 780 441', 'roberto.cano@example.test', 'Avenida America 108, Madrid', 'CH-TE-014', 'Cointra', 'Termo electrico 80L', 7, 'Equipo baño principal. Cliente demo ClimaHogar.', 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [RepuestoStock] WHERE [WorkshopId] = @workshopId AND [CodigoReferencia] = 'CH-FILT-AC')
                    INSERT INTO [RepuestoStock]
                        ([Nombre], [CodigoReferencia], [Marca], [Categoria], [Cantidad], [StockMinimo], [PrecioCompra], [PrecioVenta], [Ubicacion], [Observaciones], [IdProveedor], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Filtro split universal', 'CH-FILT-AC', 'Mundoclima', 'Climatizacion', 20, 5, 5.50, 13.00, 'CH-A1', 'Material demo para mantenimiento de aire acondicionado.', @providerId, 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [RepuestoStock] WHERE [WorkshopId] = @workshopId AND [CodigoReferencia] = 'CH-VALV-001')
                    INSERT INTO [RepuestoStock]
                        ([Nombre], [CodigoReferencia], [Marca], [Categoria], [Cantidad], [StockMinimo], [PrecioCompra], [PrecioVenta], [Ubicacion], [Observaciones], [IdProveedor], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Valvula de servicio 1/4', 'CH-VALV-001', 'RefriParts', 'Refrigeracion', 6, 3, 7.90, 19.00, 'CH-B2', 'Material demo para reparaciones de fuga.', @providerId, 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [OrdenTrabajo] WHERE [WorkshopId] = @workshopId AND [Matricula] = 'CH-AC-001' AND [Trabajo] = 'Mantenimiento preventivo de aire acondicionado')
                    INSERT INTO [OrdenTrabajo]
                        ([Cliente], [Telefono], [Matricula], [Marca], [Modelo], [Kilometraje], [Fecha], [Trabajo], [Repuestos], [ManoObra], [Estado], [Observaciones], [Facturada], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Laura Medina Ruiz', '634 210 558', 'CH-AC-001', 'Daikin', 'Split Sensira 3000 frigorias', 4, DATEADD(day, -2, @now), 'Mantenimiento preventivo de aire acondicionado', 18.00, 75.00, 'Recibido', 'Visita tecnica demo para limpieza de filtros y revision general.', 0, 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [Presupuesto] WHERE [WorkshopId] = @workshopId AND [NumeroPresupuesto] = 'CH-P-2026-0001')
                    INSERT INTO [Presupuesto]
                        ([NumeroPresupuesto], [Cliente], [Telefono], [Matricula], [Marca], [Modelo], [Kilometraje], [Fecha], [Trabajo], [Repuestos], [ManoObra], [Estado], [Observaciones], [ConvertidoEnOrden], [IdOrdenTrabajo], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('CH-P-2026-0001', 'Roberto Cano Gil', '622 780 441', 'CH-TE-014', 'Cointra', 'Termo electrico 80L', 7, DATEADD(day, -1, @now), 'Sustitucion de termo electrico', 185.00, 130.00, 'Pendiente', 'Incluye retirada del equipo antiguo y puesta en marcha.', 0, NULL, 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId);

                INSERT INTO [ServicioFrecuente] ([Nombre], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                SELECT v.[Nombre], 0, 1, 'seed:climahogar', @now, 'seed:climahogar', @now, @workshopId
                FROM (VALUES
                    ('Instalacion de aire acondicionado split'),
                    ('Mantenimiento preventivo de aire acondicionado'),
                    ('Reparacion de fuga de gas refrigerante'),
                    ('Sustitucion de termo electrico'),
                    ('Revision electrica basica'),
                    ('Desplazamiento tecnico')
                ) v([Nombre])
                WHERE NOT EXISTS (
                    SELECT 1 FROM [ServicioFrecuente] sf
                    WHERE sf.[WorkshopId] = @workshopId
                      AND LOWER(sf.[Nombre]) = LOWER(v.[Nombre])
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET NOCOUNT ON;

                DECLARE @workshopId int;
                SELECT @workshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B55667788';

                IF @workshopId IS NOT NULL
                BEGIN
                    DELETE FROM [ServicioFrecuente] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [AlertaCliente] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [FacturaEmitida] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [Presupuesto] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [OrdenTrabajo] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [RepuestoStock] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [Proveedor] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [Cliente] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [NumeradorFactura] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId;
                    DELETE FROM [Workshop] WHERE [Id] = @workshopId;
                END

                DELETE FROM [Users]
                WHERE [Email] IN (
                    'climahogar.owner@zagapro.test',
                    'climahogar.manager@zagapro.test',
                    'climahogar.tech@zagapro.test'
                );
                """);
        }
    }
}

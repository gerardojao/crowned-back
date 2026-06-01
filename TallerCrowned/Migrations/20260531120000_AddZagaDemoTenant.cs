using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddZagaDemoTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET NOCOUNT ON;

                DECLARE @now datetime = GETUTCDATE();
                DECLARE @workshopId int;
                DECLARE @ownerId int;
                DECLARE @managerId int;
                DECLARE @mechanicId int;
                DECLARE @viewerId int;
                DECLARE @providerId int;
                DECLARE @orderId int;
                DECLARE @invoiceId int;

                IF NOT EXISTS (SELECT 1 FROM [Workshop] WHERE [Nif] = 'B10987654')
                BEGIN
                    INSERT INTO [Workshop]
                        ([Nombre], [RazonSocial], [Nif], [Direccion], [Telefono], [Email], [Iban], [SerieFactura], [LogoPath], [Activo], [FechaCreacion])
                    VALUES
                        ('TGaller ZAGA', 'TGaller ZAGA S.L.', 'B10987654', 'Calle Motor 24, 28045 Madrid', '910 222 333', 'admin@tgallerzaga.test', 'ES7620770024003102575766', 'ZG', '/uploads/workshops/LogoZaga.png', 1, @now);
                END

                SELECT @workshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B10987654';
                UPDATE [Workshop]
                SET [LogoPath] = '/uploads/workshops/LogoZaga.png'
                WHERE [Id] = @workshopId;

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'zaga.owner@tallercrowned.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('zaga.owner@tallercrowned.test', 'AQAAAAIAAYagAAAAEEa+QuNCEKSXzoRkIcG8WUP4FGBrWO+n9VbXGVp4vtL9oflIo+NX8EEkGJ5seGeYGw==', 'user', 'ZAGA Owner Demo', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'zaga.manager@tallercrowned.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('zaga.manager@tallercrowned.test', 'AQAAAAIAAYagAAAAEIO7E3qAfIgU4iaw9X0KW22y2cEhjyuGkSDSFX18ibhXirbxcKbbZUPHLepeM3XLvw==', 'user', 'ZAGA Manager Demo', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'zaga.mechanic@tallercrowned.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('zaga.mechanic@tallercrowned.test', 'AQAAAAIAAYagAAAAECX4muF+Nn6SCySns0lnu1vg8DFjtaC34/PG6kQL2yN4iF/NzOFn7cinxelirfp2bw==', 'user', 'ZAGA Mechanic Demo', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'zaga.viewer@tallercrowned.test')
                    INSERT INTO [Users] ([Email], [PasswordHash], [Role], [FullName], [IsActive], [CreatedAt])
                    VALUES ('zaga.viewer@tallercrowned.test', 'AQAAAAIAAYagAAAAEC6lJrjjUczAYyq/ENWh9CcCGvi/glMlz/okRLHLCrndVyvM7PpJmfxYoOLI41vuxA==', 'user', 'ZAGA Viewer Demo', 1, @now);

                SELECT @ownerId = [Id] FROM [Users] WHERE [Email] = 'zaga.owner@tallercrowned.test';
                SELECT @managerId = [Id] FROM [Users] WHERE [Email] = 'zaga.manager@tallercrowned.test';
                SELECT @mechanicId = [Id] FROM [Users] WHERE [Email] = 'zaga.mechanic@tallercrowned.test';
                SELECT @viewerId = [Id] FROM [Users] WHERE [Email] = 'zaga.viewer@tallercrowned.test';

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @ownerId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @ownerId, 'owner', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @managerId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @managerId, 'manager', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @mechanicId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @mechanicId, 'mechanic', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [WorkshopUser] WHERE [WorkshopId] = @workshopId AND [UserId] = @viewerId)
                    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
                    VALUES (@workshopId, @viewerId, 'viewer', 1, @now);

                IF NOT EXISTS (SELECT 1 FROM [NumeradorFactura] WHERE [WorkshopId] = @workshopId AND [Serie] = 'ZG' AND [Anio] = 2026)
                    INSERT INTO [NumeradorFactura] ([WorkshopId], [OwnerKey], [Serie], [Anio], [UltimoNumero])
                    VALUES (@workshopId, CONCAT('workshop:', @workshopId), 'ZG', 2026, 1);

                IF NOT EXISTS (SELECT 1 FROM [Proveedor] WHERE [WorkshopId] = @workshopId AND [Nombre] = 'ZAGA Recambios Centro')
                    INSERT INTO [Proveedor]
                        ([Nombre], [Contacto], [Telefono], [Email], [Direccion], [Categoria], [NifCif], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('ZAGA Recambios Centro', 'Laura Martin', '911 555 720', 'recambios@zaga.test', 'Poligono Las Torres, Nave 8', 'Recambios', 'B87451230', 'Proveedor demo para pruebas multi-tenant.', 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                SELECT @providerId = [Id] FROM [Proveedor] WHERE [WorkshopId] = @workshopId AND [Nombre] = 'ZAGA Recambios Centro';

                IF NOT EXISTS (SELECT 1 FROM [Cliente] WHERE [WorkshopId] = @workshopId AND [Matricula] = '7788KZD')
                    INSERT INTO [Cliente]
                        ([Nombre], [Telefono], [Email], [Direccion], [Matricula], [Marca], [Modelo], [Kilometraje], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Marta Soler Rivas', '611 204 909', 'marta.soler@example.test', 'Calle Alcala 310, Madrid', '7788KZD', 'Toyota', 'Corolla 1.8 Hybrid', 83500, 'Cliente demo de TGaller ZAGA.', 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [Cliente] WHERE [WorkshopId] = @workshopId AND [Matricula] = '4421LPM')
                    INSERT INTO [Cliente]
                        ([Nombre], [Telefono], [Email], [Direccion], [Matricula], [Marca], [Modelo], [Kilometraje], [Observaciones], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Iker Navarro Pons', '622 118 340', 'iker.navarro@example.test', 'Avenida de Andalucia 17, Getafe', '4421LPM', 'Seat', 'Leon 1.5 TSI', 46200, 'Segundo cliente demo para comprobar aislamiento.', 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [RepuestoStock] WHERE [WorkshopId] = @workshopId AND [CodigoReferencia] = 'ZG-FILT-001')
                    INSERT INTO [RepuestoStock]
                        ([Nombre], [CodigoReferencia], [Marca], [Categoria], [Cantidad], [StockMinimo], [PrecioCompra], [PrecioVenta], [Ubicacion], [Observaciones], [IdProveedor], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Filtro de aceite Toyota 1.8', 'ZG-FILT-001', 'Bosch', 'Filtros', 12, 4, 6.80, 14.50, 'ZAGA-A1', 'Stock demo ZAGA.', @providerId, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [RepuestoStock] WHERE [WorkshopId] = @workshopId AND [CodigoReferencia] = 'ZG-PAST-004')
                    INSERT INTO [RepuestoStock]
                        ([Nombre], [CodigoReferencia], [Marca], [Categoria], [Cantidad], [StockMinimo], [PrecioCompra], [PrecioVenta], [Ubicacion], [Observaciones], [IdProveedor], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Juego pastillas freno delanteras', 'ZG-PAST-004', 'Brembo', 'Frenos', 3, 5, 28.90, 59.00, 'ZAGA-B2', 'Item bajo minimo para probar alertas visuales.', @providerId, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [OrdenTrabajo] WHERE [WorkshopId] = @workshopId AND [Matricula] = '7788KZD' AND [Trabajo] = 'Revision completa y cambio de aceite')
                    INSERT INTO [OrdenTrabajo]
                        ([Cliente], [Telefono], [Matricula], [Marca], [Modelo], [Kilometraje], [Fecha], [Trabajo], [Repuestos], [ManoObra], [Estado], [Observaciones], [Facturada], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Marta Soler Rivas', '611 204 909', '7788KZD', 'Toyota', 'Corolla 1.8 Hybrid', 83500, DATEADD(day, -3, @now), 'Revision completa y cambio de aceite', 58.00, 95.00, 'En proceso', 'Orden demo ZAGA con factura asociada.', 1, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                SELECT @orderId = [Id] FROM [OrdenTrabajo] WHERE [WorkshopId] = @workshopId AND [Matricula] = '7788KZD' AND [Trabajo] = 'Revision completa y cambio de aceite';

                IF NOT EXISTS (SELECT 1 FROM [OrdenTrabajo] WHERE [WorkshopId] = @workshopId AND [Matricula] = '4421LPM' AND [Trabajo] = 'Diagnostico ruido tren delantero')
                    INSERT INTO [OrdenTrabajo]
                        ([Cliente], [Telefono], [Matricula], [Marca], [Modelo], [Kilometraje], [Fecha], [Trabajo], [Repuestos], [ManoObra], [Estado], [Observaciones], [Facturada], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Iker Navarro Pons', '622 118 340', '4421LPM', 'Seat', 'Leon 1.5 TSI', 46200, DATEADD(day, -1, @now), 'Diagnostico ruido tren delantero', 0.00, 45.00, 'Recibido', 'Orden abierta para pruebas de filtros por taller.', 0, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [Presupuesto] WHERE [WorkshopId] = @workshopId AND [NumeroPresupuesto] = 'ZG-P-2026-0001')
                    INSERT INTO [Presupuesto]
                        ([NumeroPresupuesto], [Cliente], [Telefono], [Matricula], [Marca], [Modelo], [Kilometraje], [Fecha], [Trabajo], [Repuestos], [ManoObra], [Estado], [Observaciones], [ConvertidoEnOrden], [IdOrdenTrabajo], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('ZG-P-2026-0001', 'Iker Navarro Pons', '622 118 340', '4421LPM', 'Seat', 'Leon 1.5 TSI', 46200, DATEADD(day, -2, @now), 'Sustitucion de copelas y alineacion', 145.00, 120.00, 'Pendiente', 'Presupuesto demo no convertido.', 0, NULL, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                IF NOT EXISTS (SELECT 1 FROM [FacturaEmitida] WHERE [NumeroFactura] = 'ZG-2026-000001')
                    INSERT INTO [FacturaEmitida]
                        ([NumeroFactura], [IdOrdenTrabajo], [Fecha], [Cliente], [Dni], [DireccionCliente], [TelefonoCliente], [Matricula], [Km], [Subtotal], [Iva], [Otros], [Total], [Observaciones], [ItemsJson], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('ZG-2026-000001', @orderId, DATEADD(day, -1, @now), 'Marta Soler Rivas', '52888999T', 'Calle Alcala 310, Madrid', '611 204 909', '7788KZD', '83500', 153.00, 32.13, 0.00, 185.13, 'Factura demo ZAGA.', '[{"descripcion":"Revision completa","cantidad":1,"precio":95.00},{"descripcion":"Aceite y filtro","cantidad":1,"precio":58.00}]', 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);

                SELECT @invoiceId = [Id] FROM [FacturaEmitida] WHERE [NumeroFactura] = 'ZG-2026-000001';

                IF NOT EXISTS (SELECT 1 FROM [AlertaCliente] WHERE [WorkshopId] = @workshopId AND [Cliente] = 'Marta Soler Rivas' AND [Mensaje] = 'Llamar para seguimiento post-revision')
                    INSERT INTO [AlertaCliente]
                        ([Cliente], [Telefono], [Mensaje], [FechaAviso], [Atendida], [IdFacturaEmitida], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
                    VALUES
                        ('Marta Soler Rivas', '611 204 909', 'Llamar para seguimiento post-revision', DATEADD(day, 7, @now), 0, @invoiceId, 0, 1, 'seed:zaga', @now, 'seed:zaga', @now, @workshopId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET NOCOUNT ON;

                DECLARE @workshopId int;
                SELECT @workshopId = [Id] FROM [Workshop] WHERE [Nif] = 'B10987654';

                IF @workshopId IS NOT NULL
                BEGIN
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
                    'zaga.owner@tallercrowned.test',
                    'zaga.manager@tallercrowned.test',
                    'zaga.mechanic@tallercrowned.test',
                    'zaga.viewer@tallercrowned.test'
                );
                """);
        }
    }
}

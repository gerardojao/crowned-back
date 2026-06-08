BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaIngreso]') AND [c].[name] = N'Descripcion');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [FichaIngreso] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [FichaIngreso] ALTER COLUMN [Descripcion] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608150327_IncreaseFichaIngresoDescripcionLength', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Presupuesto] ADD [Cantidad] decimal(18,2) NOT NULL DEFAULT 1.0;
GO

ALTER TABLE [OrdenTrabajo] ADD [Cantidad] decimal(18,2) NOT NULL DEFAULT 1.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608152308_AddCantidadToOrdenTrabajoAndPresupuesto', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Presupuesto] ADD [ItemsJson] nvarchar(max) NULL;
GO

ALTER TABLE [OrdenTrabajo] ADD [ItemsJson] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608160857_AddItemsJsonToOrdenTrabajoAndPresupuesto', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Presupuesto] ADD [Dni] varchar(30) NULL;
GO

ALTER TABLE [OrdenTrabajo] ADD [Dni] varchar(30) NULL;
GO

ALTER TABLE [Cliente] ADD [Dni] varchar(30) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608164158_AddDniToCoreDocuments', N'8.0.27');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RepuestoStock]') AND [c].[name] = N'IdProveedor');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RepuestoStock] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [RepuestoStock] ALTER COLUMN [IdProveedor] int NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RepuestoStock]') AND [c].[name] = N'Cantidad');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RepuestoStock] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [RepuestoStock] ALTER COLUMN [Cantidad] decimal(18,2) NOT NULL;
GO

ALTER TABLE [RepuestoStock] ADD [Cliente] varchar(150) NULL;
GO

ALTER TABLE [RepuestoStock] ADD [EsFacturado] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [RepuestoStock] ADD [FechaFactura] datetime NULL;
GO

ALTER TABLE [RepuestoStock] ADD [IdFacturaEmitida] int NULL;
GO

ALTER TABLE [RepuestoStock] ADD [Matricula] varchar(20) NULL;
GO

ALTER TABLE [RepuestoStock] ADD [NombreProveedorSnapshot] varchar(150) NULL;
GO

ALTER TABLE [RepuestoStock] ADD [NumeroFactura] varchar(30) NULL;
GO

CREATE INDEX [IX_RepuestoStock_EsFacturado] ON [RepuestoStock] ([EsFacturado]);
GO

CREATE INDEX [IX_RepuestoStock_FechaFactura] ON [RepuestoStock] ([FechaFactura]);
GO

CREATE INDEX [IX_RepuestoStock_NumeroFactura] ON [RepuestoStock] ([NumeroFactura]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608201402_RepuestoStockFacturadoHistory', N'8.0.27');
GO

COMMIT;
GO


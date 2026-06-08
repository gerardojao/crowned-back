IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250929101611_Baseline'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Email] varchar(160) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] varchar(60) NOT NULL,
        [FullName] nvarchar(160) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ActiveSessionJti] uniqueidentifier NULL,
        [ActiveSessionExpiresAt] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250929101611_Baseline'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250929101611_Baseline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250929101611_Baseline', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250929110658_AddUsersTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250929110658_AddUsersTable', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaIngreso]') AND [c].[name] = N'Foto');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [FichaIngreso] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [FichaIngreso] ALTER COLUMN [Foto] varchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [FechaModificacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UsuarioCreacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UsuarioModificacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaEgreso]') AND [c].[name] = N'Foto');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [FichaEgreso] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [FichaEgreso] ALTER COLUMN [Foto] varchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [FechaModificacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UsuarioCreacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UsuarioModificacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    CREATE TABLE [PasswordResets] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [TokenHash] nvarchar(88) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [UsedAt] datetime2 NULL,
        [RequestIp] nvarchar(64) NULL,
        [RequestUserAgent] nvarchar(256) NULL,
        CONSTRAINT [PK_PasswordResets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PasswordResets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordResets_TokenHash] ON [PasswordResets] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    CREATE INDEX [IX_PasswordResets_UserId] ON [PasswordResets] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001114306_AuditableShadowProps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251001114306_AuditableShadowProps', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [FechaModificacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UserId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UsuarioCreacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UsuarioModificacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    CREATE INDEX [IX_FichaEgreso_UserId_Eliminado_Fecha] ON [FichaEgreso] ([UserId], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [FechaModificacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UserId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UsuarioCreacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UsuarioModificacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    CREATE INDEX [IX_FichaIngreso_UserId_Eliminado_Fecha] ON [FichaIngreso] ([UserId], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001124852_AuditableAndUserOwnerShadowProps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251001124852_AuditableAndUserOwnerShadowProps', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaIngreso]') AND [c].[name] = N'Eliminado');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [FichaIngreso] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [FichaIngreso] ADD DEFAULT CAST(0 AS bit) FOR [Eliminado];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [UserId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [UserId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    CREATE INDEX [IX_FichaIngreso_UserId_Eliminado_Fecha] ON [FichaIngreso] ([UserId], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    CREATE INDEX [IX_FichaEgreso_UserId_Eliminado_Fecha] ON [FichaEgreso] ([UserId], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001142309_AuditableAndUserOwnerShadowProps2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251001142309_AuditableAndUserOwnerShadowProps2', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    DROP INDEX [IX_FichaIngreso_UserId_Eliminado_Fecha] ON [FichaIngreso];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    DROP INDEX [IX_FichaEgreso_UserId_Eliminado_Fecha] ON [FichaEgreso];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaIngreso]') AND [c].[name] = N'UserId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [FichaIngreso] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [FichaIngreso] DROP COLUMN [UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaEgreso]') AND [c].[name] = N'UserId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [FichaEgreso] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [FichaEgreso] DROP COLUMN [UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    CREATE INDEX [IX_FichaIngreso_UsuarioCreacion_Eliminado_Fecha] ON [FichaIngreso] ([UsuarioCreacion], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    CREATE INDEX [IX_FichaEgreso_UsuarioCreacion_Eliminado_Fecha] ON [FichaEgreso] ([UsuarioCreacion], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251001144805_AuditableAndUserOwnerShadowProps3'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251001144805_AuditableAndUserOwnerShadowProps3', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526132047_AddOrdenTrabajo'
)
BEGIN
    CREATE TABLE [OrdenTrabajo] (
        [Id] int NOT NULL IDENTITY,
        [Cliente] varchar(150) NOT NULL,
        [Telefono] varchar(30) NULL,
        [Matricula] varchar(20) NOT NULL,
        [Marca] varchar(80) NULL,
        [Modelo] varchar(80) NOT NULL,
        [Kilometraje] int NULL,
        [Fecha] datetime NOT NULL,
        [Trabajo] varchar(500) NOT NULL,
        [Repuestos] decimal(18,2) NOT NULL,
        [ManoObra] decimal(18,2) NOT NULL,
        [Estado] varchar(50) NOT NULL,
        [Observaciones] varchar(500) NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_OrdenTrabajo] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526132047_AddOrdenTrabajo'
)
BEGIN
    CREATE INDEX [IX_OrdenTrabajo_Matricula] ON [OrdenTrabajo] ([Matricula]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526132047_AddOrdenTrabajo'
)
BEGIN
    CREATE INDEX [IX_OrdenTrabajo_UsuarioCreacion_Eliminado_Fecha] ON [OrdenTrabajo] ([UsuarioCreacion], [Eliminado], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526132047_AddOrdenTrabajo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260526132047_AddOrdenTrabajo', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526160551_AddCliente'
)
BEGIN
    CREATE TABLE [Cliente] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] varchar(150) NOT NULL,
        [Telefono] varchar(30) NOT NULL,
        [Email] varchar(150) NULL,
        [Direccion] varchar(250) NULL,
        [Matricula] varchar(20) NOT NULL,
        [Marca] varchar(80) NULL,
        [Modelo] varchar(80) NOT NULL,
        [Kilometraje] int NULL,
        [Observaciones] varchar(500) NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_Cliente] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526160551_AddCliente'
)
BEGIN
    CREATE INDEX [IX_Cliente_Matricula] ON [Cliente] ([Matricula]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526160551_AddCliente'
)
BEGIN
    CREATE INDEX [IX_Cliente_Telefono] ON [Cliente] ([Telefono]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526160551_AddCliente'
)
BEGIN
    CREATE INDEX [IX_Cliente_UsuarioCreacion_Eliminado] ON [Cliente] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526160551_AddCliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260526160551_AddCliente', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527122257_AddProveedor'
)
BEGIN
    CREATE TABLE [Proveedor] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] varchar(150) NOT NULL,
        [Contacto] varchar(150) NULL,
        [Telefono] varchar(30) NULL,
        [Email] varchar(150) NULL,
        [Direccion] varchar(250) NULL,
        [Categoria] varchar(80) NULL,
        [NifCif] varchar(30) NULL,
        [Observaciones] varchar(500) NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_Proveedor] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527122257_AddProveedor'
)
BEGIN
    CREATE INDEX [IX_Proveedor_Categoria] ON [Proveedor] ([Categoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527122257_AddProveedor'
)
BEGIN
    CREATE INDEX [IX_Proveedor_Nombre] ON [Proveedor] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527122257_AddProveedor'
)
BEGIN
    CREATE INDEX [IX_Proveedor_UsuarioCreacion_Eliminado] ON [Proveedor] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527122257_AddProveedor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527122257_AddProveedor', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    CREATE TABLE [RepuestoStock] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] varchar(150) NOT NULL,
        [CodigoReferencia] varchar(80) NULL,
        [Marca] varchar(80) NULL,
        [Categoria] varchar(80) NULL,
        [Cantidad] int NOT NULL,
        [StockMinimo] int NOT NULL DEFAULT 3,
        [PrecioCompra] decimal(18,2) NOT NULL,
        [PrecioVenta] decimal(18,2) NULL,
        [Ubicacion] varchar(100) NULL,
        [Observaciones] varchar(500) NULL,
        [IdProveedor] int NOT NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        CONSTRAINT [PK_RepuestoStock] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RepuestoStock_Proveedor_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [Proveedor] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_Categoria] ON [RepuestoStock] ([Categoria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_CodigoReferencia] ON [RepuestoStock] ([CodigoReferencia]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_IdProveedor] ON [RepuestoStock] ([IdProveedor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_Nombre] ON [RepuestoStock] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527133458_AddRepuestoStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527133458_AddRepuestoStock', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [FechaModificacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [UsuarioCreacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [UsuarioModificacion] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_UsuarioCreacion_Eliminado] ON [RepuestoStock] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527143734_AddAuditFieldsToRepuestoStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527143734_AddAuditFieldsToRepuestoStock', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527145453_AddActivoToRepuestoStock'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260527145453_AddActivoToRepuestoStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260527145453_AddActivoToRepuestoStock', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529072003_AddNumeradorFactura'
)
BEGIN
    CREATE TABLE [NumeradorFactura] (
        [Id] int NOT NULL IDENTITY,
        [Anio] int NOT NULL,
        [UltimoNumero] int NOT NULL,
        CONSTRAINT [PK_NumeradorFactura] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529072003_AddNumeradorFactura'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumeradorFactura_Anio] ON [NumeradorFactura] ([Anio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529072003_AddNumeradorFactura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529072003_AddNumeradorFactura', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529081337_AddFacturadaToOrdenTrabajo'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD [Facturada] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529081337_AddFacturadaToOrdenTrabajo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529081337_AddFacturadaToOrdenTrabajo', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529090710_AddFacturaEmitida'
)
BEGIN
    CREATE TABLE [FacturaEmitida] (
        [Id] int NOT NULL IDENTITY,
        [NumeroFactura] varchar(30) NOT NULL,
        [IdOrdenTrabajo] int NULL,
        [Fecha] datetime2 NOT NULL,
        [Cliente] varchar(150) NOT NULL,
        [Dni] varchar(30) NULL,
        [DireccionCliente] varchar(250) NULL,
        [TelefonoCliente] varchar(30) NULL,
        [Matricula] varchar(20) NULL,
        [Km] varchar(30) NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Iva] decimal(18,2) NOT NULL,
        [Otros] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Observaciones] varchar(1000) NULL,
        [ItemsJson] nvarchar(max) NOT NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_FacturaEmitida] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529090710_AddFacturaEmitida'
)
BEGIN
    CREATE INDEX [IX_FacturaEmitida_IdOrdenTrabajo] ON [FacturaEmitida] ([IdOrdenTrabajo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529090710_AddFacturaEmitida'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FacturaEmitida_NumeroFactura] ON [FacturaEmitida] ([NumeroFactura]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529090710_AddFacturaEmitida'
)
BEGIN
    CREATE INDEX [IX_FacturaEmitida_UsuarioCreacion_Eliminado] ON [FacturaEmitida] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529090710_AddFacturaEmitida'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529090710_AddFacturaEmitida', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115304_AddAlertaCliente'
)
BEGIN
    CREATE TABLE [AlertaCliente] (
        [Id] int NOT NULL IDENTITY,
        [Cliente] varchar(150) NOT NULL,
        [Telefono] varchar(30) NULL,
        [Mensaje] varchar(500) NOT NULL,
        [FechaAviso] datetime NOT NULL,
        [Atendida] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IdFacturaEmitida] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_AlertaCliente] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115304_AddAlertaCliente'
)
BEGIN
    CREATE INDEX [IX_AlertaCliente_Atendida] ON [AlertaCliente] ([Atendida]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115304_AddAlertaCliente'
)
BEGIN
    CREATE INDEX [IX_AlertaCliente_FechaAviso] ON [AlertaCliente] ([FechaAviso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115304_AddAlertaCliente'
)
BEGIN
    CREATE INDEX [IX_AlertaCliente_UsuarioCreacion_Eliminado] ON [AlertaCliente] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115304_AddAlertaCliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529115304_AddAlertaCliente', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    CREATE TABLE [Presupuesto] (
        [Id] int NOT NULL IDENTITY,
        [NumeroPresupuesto] varchar(30) NOT NULL,
        [Cliente] varchar(150) NOT NULL,
        [Telefono] varchar(30) NULL,
        [Matricula] varchar(20) NOT NULL,
        [Marca] varchar(80) NULL,
        [Modelo] varchar(80) NOT NULL,
        [Kilometraje] int NULL,
        [Fecha] datetime2 NOT NULL,
        [Trabajo] varchar(1000) NOT NULL,
        [Repuestos] decimal(18,2) NOT NULL,
        [ManoObra] decimal(18,2) NOT NULL,
        [Estado] varchar(30) NOT NULL DEFAULT 'Pendiente',
        [Observaciones] varchar(1000) NULL,
        [ConvertidoEnOrden] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IdOrdenTrabajo] int NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(64) NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        CONSTRAINT [PK_Presupuesto] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    CREATE INDEX [IX_Presupuesto_Estado] ON [Presupuesto] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    CREATE INDEX [IX_Presupuesto_Matricula] ON [Presupuesto] ([Matricula]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Presupuesto_NumeroPresupuesto] ON [Presupuesto] ([NumeroPresupuesto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    CREATE INDEX [IX_Presupuesto_UsuarioCreacion_Eliminado] ON [Presupuesto] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529161303_AddPresupuesto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529161303_AddPresupuesto', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530103002_AddScopedInvoiceNumbering'
)
BEGIN
    DROP INDEX [IX_NumeradorFactura_Anio] ON [NumeradorFactura];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530103002_AddScopedInvoiceNumbering'
)
BEGIN
    ALTER TABLE [NumeradorFactura] ADD [OwnerKey] varchar(64) NOT NULL DEFAULT 'legacy';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530103002_AddScopedInvoiceNumbering'
)
BEGIN
    ALTER TABLE [NumeradorFactura] ADD [Serie] varchar(20) NOT NULL DEFAULT 'A';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530103002_AddScopedInvoiceNumbering'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumeradorFactura_OwnerKey_Serie_Anio] ON [NumeradorFactura] ([OwnerKey], [Serie], [Anio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530103002_AddScopedInvoiceNumbering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530103002_AddScopedInvoiceNumbering', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Proveedor] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Presupuesto] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [NumeradorFactura] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Ingreso] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FacturaEmitida] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Egresos] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Cliente] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [AlertaCliente] ADD [WorkshopId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE TABLE [Workshop] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] varchar(150) NOT NULL,
        [RazonSocial] varchar(180) NOT NULL,
        [Nif] varchar(30) NOT NULL,
        [Direccion] varchar(250) NOT NULL,
        [Telefono] varchar(50) NULL,
        [Email] varchar(150) NULL,
        [Iban] varchar(50) NULL,
        [SerieFactura] varchar(20) NOT NULL DEFAULT 'A',
        [LogoPath] varchar(300) NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime NOT NULL,
        CONSTRAINT [PK_Workshop] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN

    SET IDENTITY_INSERT [Workshop] ON;
    IF NOT EXISTS (SELECT 1 FROM [Workshop] WHERE [Id] = 1)
    BEGIN
        INSERT INTO [Workshop]
            ([Id], [Nombre], [RazonSocial], [Nif], [Direccion], [Telefono], [Email], [Iban], [SerieFactura], [LogoPath], [Activo], [FechaCreacion])
        VALUES
            (1, 'Multiservicios Crower', 'JUAN CARLOS FERNANDEZ SILVA', '61407055E', 'CALLE ALCACER 63 D, Albal, 46470', '960057935/655042253', 'multiservicioscrower@gmail.com', 'ES69 2100 4014 9122 0012 3843', 'A', '/uploads/workshops/LogoTallerCrowned.png', 1, GETUTCDATE());
    END
    SET IDENTITY_INSERT [Workshop] OFF;

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE TABLE [WorkshopUser] (
        [Id] int NOT NULL IDENTITY,
        [WorkshopId] int NOT NULL,
        [UserId] int NOT NULL,
        [Role] varchar(50) NOT NULL DEFAULT 'owner',
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaCreacion] datetime NOT NULL,
        CONSTRAINT [PK_WorkshopUser] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkshopUser_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkshopUser_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN

    INSERT INTO [WorkshopUser] ([WorkshopId], [UserId], [Role], [Activo], [FechaCreacion])
    SELECT 1, [Id], 'owner', 1, GETUTCDATE()
    FROM [Users] u
    WHERE NOT EXISTS (
        SELECT 1
        FROM [WorkshopUser] wu
        WHERE wu.[WorkshopId] = 1 AND wu.[UserId] = u.[Id]
    );

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_RepuestoStock_WorkshopId] ON [RepuestoStock] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Proveedor_WorkshopId] ON [Proveedor] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Presupuesto_WorkshopId] ON [Presupuesto] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_OrdenTrabajo_WorkshopId] ON [OrdenTrabajo] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_NumeradorFactura_WorkshopId_Serie_Anio] ON [NumeradorFactura] ([WorkshopId], [Serie], [Anio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Ingreso_WorkshopId] ON [Ingreso] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_FichaIngreso_WorkshopId] ON [FichaIngreso] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_FichaEgreso_WorkshopId] ON [FichaEgreso] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_FacturaEmitida_WorkshopId] ON [FacturaEmitida] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Egresos_WorkshopId] ON [Egresos] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Cliente_WorkshopId] ON [Cliente] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_AlertaCliente_WorkshopId] ON [AlertaCliente] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Workshop_Activo] ON [Workshop] ([Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_Workshop_Nif] ON [Workshop] ([Nif]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE INDEX [IX_WorkshopUser_UserId_Activo] ON [WorkshopUser] ([UserId], [Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WorkshopUser_WorkshopId_UserId] ON [WorkshopUser] ([WorkshopId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [AlertaCliente] ADD CONSTRAINT [FK_AlertaCliente_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Cliente] ADD CONSTRAINT [FK_Cliente_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Egresos] ADD CONSTRAINT [FK_Egresos_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FacturaEmitida] ADD CONSTRAINT [FK_FacturaEmitida_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FichaEgreso] ADD CONSTRAINT [FK_FichaEgreso_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [FichaIngreso] ADD CONSTRAINT [FK_FichaIngreso_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Ingreso] ADD CONSTRAINT [FK_Ingreso_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [NumeradorFactura] ADD CONSTRAINT [FK_NumeradorFactura_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD CONSTRAINT [FK_OrdenTrabajo_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Presupuesto] ADD CONSTRAINT [FK_Presupuesto_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [Proveedor] ADD CONSTRAINT [FK_Proveedor_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    ALTER TABLE [RepuestoStock] ADD CONSTRAINT [FK_RepuestoStock_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530110702_AddWorkshopTenantPhase4A'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530110702_AddWorkshopTenantPhase4A', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530113047_ActivateWorkshopTenantPhase4B'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530113047_ActivateWorkshopTenantPhase4B'
)
BEGIN
    CREATE INDEX [IX_NumeradorFactura_OwnerKey_Serie_Anio] ON [NumeradorFactura] ([OwnerKey], [Serie], [Anio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530113047_ActivateWorkshopTenantPhase4B'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530113047_ActivateWorkshopTenantPhase4B'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumeradorFactura_WorkshopId_Serie_Anio] ON [NumeradorFactura] ([WorkshopId], [Serie], [Anio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530113047_ActivateWorkshopTenantPhase4B'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530113047_ActivateWorkshopTenantPhase4B', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531120000_AddZagaDemoTenant'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531120000_AddZagaDemoTenant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260531120000_AddZagaDemoTenant', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
    ALTER TABLE [Workshop] ADD [MaxUsers] int NOT NULL DEFAULT 3;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
    ALTER TABLE [Workshop] ADD [FooterText] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
    ALTER TABLE [Workshop] ADD [PrivacyPolicyText] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
    ALTER TABLE [Workshop] ADD [TermsText] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531143000_AddWorkshopAdminControls'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260531143000_AddWorkshopAdminControls', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531170000_ScopeBudgetNumberByWorkshop'
)
BEGIN
    DROP INDEX [IX_Presupuesto_NumeroPresupuesto] ON [Presupuesto];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531170000_ScopeBudgetNumberByWorkshop'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Presupuesto_WorkshopId_NumeroPresupuesto] ON [Presupuesto] ([WorkshopId], [NumeroPresupuesto]) WHERE [WorkshopId] IS NOT NULL AND [NumeroPresupuesto] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531170000_ScopeBudgetNumberByWorkshop'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260531170000_ScopeBudgetNumberByWorkshop', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN
    CREATE TABLE [ServicioFrecuente] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] varchar(180) NOT NULL,
        [Eliminado] bit NOT NULL DEFAULT CAST(0 AS bit),
        [FechaEliminacion] datetime2 NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [UsuarioCreacion] nvarchar(64) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioModificacion] nvarchar(64) NULL,
        [FechaModificacion] datetime2 NOT NULL,
        [WorkshopId] int NOT NULL DEFAULT 1,
        CONSTRAINT [PK_ServicioFrecuente] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServicioFrecuente_Workshop_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshop] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN
    CREATE INDEX [IX_ServicioFrecuente_UsuarioCreacion_Eliminado] ON [ServicioFrecuente] ([UsuarioCreacion], [Eliminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN
    CREATE INDEX [IX_ServicioFrecuente_WorkshopId] ON [ServicioFrecuente] ([WorkshopId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ServicioFrecuente_WorkshopId_Nombre] ON [ServicioFrecuente] ([WorkshopId], [Nombre]) WHERE [WorkshopId] IS NOT NULL AND [Nombre] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN

    DECLARE @now datetime2 = SYSUTCDATETIME();

    INSERT INTO [ServicioFrecuente] ([Nombre], [Eliminado], [Activo], [UsuarioCreacion], [FechaCreacion], [UsuarioModificacion], [FechaModificacion], [WorkshopId])
    SELECT v.[Nombre], 0, 1, 'seed:servicios', @now, 'seed:servicios', @now, w.[Id]
    FROM [Workshop] w
    CROSS JOIN (VALUES
        ('Servicio cambio de aceite y filtro'),
        ('Cambio de pastillas de frenos'),
        ('Cambio de rodamientos delanteros'),
        ('Cambio de amortiguadores'),
        ('Mano de obra'),
        ('Repuestos')
    ) v([Nombre])
    WHERE NOT EXISTS (
        SELECT 1
        FROM [ServicioFrecuente] sf
        WHERE sf.[WorkshopId] = w.[Id]
          AND LOWER(sf.[Nombre]) = LOWER(v.[Nombre])
    );

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601120000_AddServicioFrecuente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601120000_AddServicioFrecuente', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601133000_AddClimaHogarDemoTenant'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601133000_AddClimaHogarDemoTenant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601133000_AddClimaHogarDemoTenant', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601143000_AddWorkshopBusinessProfile'
)
BEGIN
    ALTER TABLE [Workshop] ADD [BusinessType] varchar(50) NOT NULL DEFAULT 'automotive';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601143000_AddWorkshopBusinessProfile'
)
BEGIN
    ALTER TABLE [Workshop] ADD [TerminologyProfile] varchar(50) NOT NULL DEFAULT 'automotive';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601143000_AddWorkshopBusinessProfile'
)
BEGIN

    UPDATE Workshop
    SET BusinessType = 'technical_services',
        TerminologyProfile = 'equipment_service'
    WHERE Nif = 'B55667788' OR Nombre LIKE '%ClimaHogar%';

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601143000_AddWorkshopBusinessProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601143000_AddWorkshopBusinessProfile', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260603181828_AddEgresoTipoGasto'
)
BEGIN
    IF COL_LENGTH('Egresos', 'TipoGasto') IS NULL
        ALTER TABLE [Egresos] ADD [TipoGasto] varchar(20) NOT NULL CONSTRAINT [DF_Egresos_TipoGasto] DEFAULT 'variable';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260603181828_AddEgresoTipoGasto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260603181828_AddEgresoTipoGasto', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604073608_AddWorkshopFeatureModulesEf'
)
BEGIN
    ALTER TABLE [Workshop] ADD [EnableInvoiceExport] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604073608_AddWorkshopFeatureModulesEf'
)
BEGIN
    ALTER TABLE [Workshop] ADD [EnableProfitAndLoss] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604073608_AddWorkshopFeatureModulesEf'
)
BEGIN
    ALTER TABLE [Workshop] ADD [EnableWhatsappAlerts] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260604073608_AddWorkshopFeatureModulesEf'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260604073608_AddWorkshopFeatureModulesEf', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608150327_IncreaseFichaIngresoDescripcionLength'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FichaIngreso]') AND [c].[name] = N'Descripcion');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [FichaIngreso] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [FichaIngreso] ALTER COLUMN [Descripcion] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608150327_IncreaseFichaIngresoDescripcionLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260608150327_IncreaseFichaIngresoDescripcionLength', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608152308_AddCantidadToOrdenTrabajoAndPresupuesto'
)
BEGIN
    ALTER TABLE [Presupuesto] ADD [Cantidad] decimal(18,2) NOT NULL DEFAULT 1.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608152308_AddCantidadToOrdenTrabajoAndPresupuesto'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD [Cantidad] decimal(18,2) NOT NULL DEFAULT 1.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608152308_AddCantidadToOrdenTrabajoAndPresupuesto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260608152308_AddCantidadToOrdenTrabajoAndPresupuesto', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608160857_AddItemsJsonToOrdenTrabajoAndPresupuesto'
)
BEGIN
    ALTER TABLE [Presupuesto] ADD [ItemsJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608160857_AddItemsJsonToOrdenTrabajoAndPresupuesto'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD [ItemsJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608160857_AddItemsJsonToOrdenTrabajoAndPresupuesto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260608160857_AddItemsJsonToOrdenTrabajoAndPresupuesto', N'8.0.27');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608164158_AddDniToCoreDocuments'
)
BEGIN
    ALTER TABLE [Presupuesto] ADD [Dni] varchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608164158_AddDniToCoreDocuments'
)
BEGIN
    ALTER TABLE [OrdenTrabajo] ADD [Dni] varchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608164158_AddDniToCoreDocuments'
)
BEGIN
    ALTER TABLE [Cliente] ADD [Dni] varchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608164158_AddDniToCoreDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260608164158_AddDniToCoreDocuments', N'8.0.27');
END;
GO

COMMIT;
GO


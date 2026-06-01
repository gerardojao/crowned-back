using System;
using System.Collections.Generic;
using FamilyApp.Models;
using Microsoft.EntityFrameworkCore;
using TallerCrowned.Models;

namespace FamilyApp.Data;

public partial class dbContext : DbContext
{
    private readonly ICurrentUserService? _current;

    public dbContext() { } // usado por herramientas

    public dbContext(DbContextOptions<dbContext> options, ICurrentUserService? current = null)
        : base(options)
    {
        _current = current;
    }

    public virtual DbSet<Egreso> Egresos { get; set; }
    public virtual DbSet<FichaEgreso> FichaEgresos { get; set; }
    public virtual DbSet<FichaIngreso> FichaIngresos { get; set; }
    public virtual DbSet<Ingreso> Ingresos { get; set; }
    public virtual DbSet<AppUser> Users { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; } = default!;
    public virtual DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Proveedor> Proveedores { get; set; }
    public virtual DbSet<RepuestoStock> RepuestosStock { get; set; }

    public virtual DbSet<NumeradorFactura> NumeradoresFactura { get; set; }

    public virtual DbSet<FacturaEmitida> FacturasEmitidas { get; set; }
    public virtual DbSet<AlertaCliente> AlertasClientes { get; set; }
    public virtual DbSet<Presupuesto> Presupuestos { get; set; }
    public virtual DbSet<ServicioFrecuente> ServiciosFrecuentes { get; set; }
    public virtual DbSet<Workshop> Workshops { get; set; }
    public virtual DbSet<WorkshopUser> WorkshopUsers { get; set; }

    // --- Auditoría automática ---
    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(ct);
    }

    private void ApplyAudit()
    {
        var now = DateTime.UtcNow;
        var uidStr = _current?.UserIdOrEmail ?? "system";

        foreach (var e in ChangeTracker.Entries())
        {
            // Solo entidades con props sombra de auditoría
            bool hasAudit =
                e.Metadata.FindProperty("UsuarioCreacion") != null &&
                e.Metadata.FindProperty("FechaCreacion") != null &&
                e.Metadata.FindProperty("UsuarioModificacion") != null &&
                e.Metadata.FindProperty("FechaModificacion") != null &&
                e.Metadata.FindProperty("Activo") != null;

            if (!hasAudit) continue;

            if (e.State == EntityState.Added)
            {
                e.Property("Activo").CurrentValue = true;
                e.Property("UsuarioCreacion").CurrentValue = uidStr;
                e.Property("FechaCreacion").CurrentValue = now;
                e.Property("UsuarioModificacion").CurrentValue = uidStr;
                e.Property("FechaModificacion").CurrentValue = now;

                // Asignar UserId (prop sombra) si existe
                var pUserId = e.Metadata.FindProperty("UserId");
                if (pUserId != null && _current?.UserIdInt is int uid)
                {
                    e.Property("UserId").CurrentValue = uid;
                }
            }
            else if (e.State == EntityState.Modified)
            {
                // No tocar creación
                e.Property("UsuarioCreacion").IsModified = false;
                e.Property("FechaCreacion").IsModified = false;

                e.Property("UsuarioModificacion").CurrentValue = uidStr;
                e.Property("FechaModificacion").CurrentValue = now;
            }
            else if (e.State == EntityState.Deleted)
            {
                // Soft-delete si la entidad lo soporta
                var hasSoft =
                    e.Metadata.FindProperty("Eliminado") != null &&
                    e.Metadata.FindProperty("FechaEliminacion") != null;

                e.State = EntityState.Modified;
                e.Property("Activo").CurrentValue = false;
                e.Property("UsuarioModificacion").CurrentValue = uidStr;
                e.Property("FechaModificacion").CurrentValue = now;

                if (hasSoft)
                {
                    e.CurrentValues["Eliminado"] = true;
                    e.CurrentValues["FechaEliminacion"] = now;
                    e.Property("Eliminado").IsModified = true;
                    e.Property("FechaEliminacion").IsModified = true;
                }
            }
        }
    }
    // --- fin auditoría automática ---

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=familyApp;Trusted_Connection=true;MultipleActiveResultSets=true;Encrypt=false");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AppUser
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsUnicode(false);
            entity.Property(u => u.Role).IsUnicode(false);
        });

        modelBuilder.Entity<Workshop>(b =>
        {
            b.ToTable("Workshop");

            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.RazonSocial)
                .HasMaxLength(180)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Nif)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);

            b.Property(x => x.Email)
                .HasMaxLength(150)
                .IsUnicode(false);

            b.Property(x => x.Iban)
                .HasMaxLength(50)
                .IsUnicode(false);

            b.Property(x => x.SerieFactura)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .IsRequired();

            b.Property(x => x.LogoPath)
                .HasMaxLength(300)
                .IsUnicode(false);

            b.Property(x => x.BusinessType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("automotive")
                .IsRequired();

            b.Property(x => x.TerminologyProfile)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("automotive")
                .IsRequired();

            b.Property(x => x.MaxUsers)
                .HasDefaultValue(3)
                .IsRequired();

            b.Property(x => x.FooterText)
                .HasMaxLength(300);

            b.Property(x => x.PrivacyPolicyText)
                .HasColumnType("nvarchar(max)");

            b.Property(x => x.TermsText)
                .HasColumnType("nvarchar(max)");

            b.Property(x => x.Activo).HasDefaultValue(true);
            b.Property(x => x.FechaCreacion).HasColumnType("datetime");

            b.HasIndex(x => x.Nif);
            b.HasIndex(x => x.Activo);
        });

        modelBuilder.Entity<WorkshopUser>(b =>
        {
            b.ToTable("WorkshopUser");

            b.HasKey(x => x.Id);

            b.Property(x => x.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("owner")
                .IsRequired();

            b.Property(x => x.Activo).HasDefaultValue(true);
            b.Property(x => x.FechaCreacion).HasColumnType("datetime");

            b.HasOne(x => x.Workshop)
                .WithMany()
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.WorkshopId, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.UserId, x.Activo });
        });

        // Egreso catálogo
        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            ConfigureWorkshopShadow<Egreso>(entity);
        });

        // Ingreso catálogo
        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.ToTable("Ingreso");
            entity.Property(e => e.NombreIngreso).HasMaxLength(100).IsUnicode(false);
            ConfigureWorkshopShadow<Ingreso>(entity);
        });

        // ---------- FichaEgreso ----------
        modelBuilder.Entity<FichaEgreso>(b =>
        {
            b.ToTable("FichaEgreso");

            // columnas normales
            b.Property(e => e.Fecha).HasColumnType("datetime");
            b.Property(e => e.Foto).HasMaxLength(255).IsUnicode(false);
            b.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            b.Property(e => e.Mes).HasMaxLength(10).IsUnicode(false);
            b.Property(e => e.Descripcion).HasMaxLength(50).IsRequired(false);
            b.Property(e => e.Eliminado).HasDefaultValue(false);

            // auditoría (sombra)
            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            // dueño (sombra)
            ConfigureWorkshopShadow<FichaEgreso>(b);
            b.HasIndex("UsuarioCreacion", "Eliminado", "Fecha");
        });

        // ---------- FichaIngreso ----------
        modelBuilder.Entity<FichaIngreso>(b =>
        {
            b.ToTable("FichaIngreso");

            // columnas normales
            b.Property(e => e.Fecha).HasColumnType("datetime");
            b.Property(e => e.Foto).HasMaxLength(255).IsUnicode(false);
            b.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            b.Property(e => e.Mes).HasMaxLength(10).IsUnicode(false);
            b.Property(e => e.Descripcion).HasMaxLength(50).IsRequired(false);
            b.Property(e => e.Eliminado).HasDefaultValue(false);

            // auditoría (sombra)
            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            // dueño (sombra)
            ConfigureWorkshopShadow<FichaIngreso>(b);
            b.HasIndex("UsuarioCreacion", "Eliminado", "Fecha");
        });

        modelBuilder.Entity<OrdenTrabajo>(b =>
        {
            b.ToTable("OrdenTrabajo");

            b.Property(e => e.Cliente)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Matricula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Marca)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Modelo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Kilometraje)
                .IsRequired(false);

            b.Property(e => e.Fecha)
                .HasColumnType("datetime");

            b.Property(e => e.Trabajo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Repuestos)
                .HasColumnType("decimal(18, 2)");

            b.Property(e => e.ManoObra)
                .HasColumnType("decimal(18, 2)");

            b.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Facturada)
                 .HasDefaultValue(false);

            b.Property(e => e.Eliminado)
                .HasDefaultValue(false);

            // auditoría sombra
            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<OrdenTrabajo>(b);
            b.HasIndex("UsuarioCreacion", "Eliminado", "Fecha");
            b.HasIndex(e => e.Matricula);
        });

        modelBuilder.Entity<Cliente>(b =>
        {
            b.ToTable("Cliente");

            b.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Matricula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Marca)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Modelo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Eliminado)
                .HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<Cliente>(b);
            b.HasIndex(e => e.Matricula);
            b.HasIndex(e => e.Telefono);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        modelBuilder.Entity<Proveedor>(b =>
        {
            b.ToTable("Proveedor");

            b.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(e => e.Contacto)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Categoria)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.NifCif)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired(false);

            b.Property(e => e.Eliminado)
                .HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<Proveedor>(b);
            b.HasIndex(e => e.Nombre);
            b.HasIndex(e => e.Categoria);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        modelBuilder.Entity<RepuestoStock>(b =>
        {
            b.ToTable("RepuestoStock");

            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.CodigoReferencia)
                .HasMaxLength(80)
                .IsUnicode(false);

            b.Property(x => x.Marca)
                .HasMaxLength(80)
                .IsUnicode(false);

            b.Property(x => x.Categoria)
                .HasMaxLength(80)
                .IsUnicode(false);

            b.Property(x => x.Ubicacion)
                .HasMaxLength(100)
                .IsUnicode(false);

            b.Property(x => x.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false);

            b.Property(x => x.PrecioCompra)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.PrecioVenta)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.StockMinimo)
                .HasDefaultValue(3);

            b.Property(x => x.Eliminado)
                .HasDefaultValue(false);

            b.HasOne(x => x.Proveedor)
                .WithMany()
                .HasForeignKey(x => x.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Nombre);
            b.HasIndex(x => x.Categoria);
            b.HasIndex(x => x.CodigoReferencia);
            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<RepuestoStock>(b);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        modelBuilder.Entity<NumeradorFactura>(b =>
        {
            b.ToTable("NumeradorFactura");

            b.HasKey(x => x.Id);

            b.Property(x => x.WorkshopId)
                .HasDefaultValue(1)
                .IsRequired();

            b.Property(x => x.OwnerKey)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValue("legacy")
                .IsRequired();

            b.Property(x => x.Serie)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .IsRequired();

            b.Property(x => x.Anio)
                .IsRequired();

            b.Property(x => x.UltimoNumero)
                .IsRequired();

            b.HasIndex(x => new { x.WorkshopId, x.Serie, x.Anio })
                .IsUnique();

            b.HasOne<Workshop>()
                .WithMany()
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.OwnerKey, x.Serie, x.Anio });
        });

        modelBuilder.Entity<FacturaEmitida>(b =>
        {
            b.ToTable("FacturaEmitida");

            b.HasKey(x => x.Id);

            b.Property(x => x.NumeroFactura)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Cliente)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Dni).HasMaxLength(30).IsUnicode(false);
            b.Property(x => x.DireccionCliente).HasMaxLength(250).IsUnicode(false);
            b.Property(x => x.TelefonoCliente).HasMaxLength(30).IsUnicode(false);
            b.Property(x => x.Matricula).HasMaxLength(20).IsUnicode(false);
            b.Property(x => x.Km).HasMaxLength(30).IsUnicode(false);
            b.Property(x => x.Observaciones).HasMaxLength(1000).IsUnicode(false);

            b.Property(x => x.ItemsJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.Iva).HasColumnType("decimal(18,2)");
            b.Property(x => x.Otros).HasColumnType("decimal(18,2)");
            b.Property(x => x.Total).HasColumnType("decimal(18,2)");

            b.Property(x => x.Eliminado).HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<FacturaEmitida>(b);
            b.HasIndex(x => x.NumeroFactura).IsUnique();
            b.HasIndex(x => x.IdOrdenTrabajo);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });
        modelBuilder.Entity<AlertaCliente>(b =>
        {
            b.ToTable("AlertaCliente");

            b.HasKey(x => x.Id);

            b.Property(x => x.Cliente)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false);

            b.Property(x => x.Mensaje)
                .HasMaxLength(500)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.FechaAviso)
                .HasColumnType("datetime");

            b.Property(x => x.Atendida)
                .HasDefaultValue(false);

            b.Property(x => x.Eliminado)
                .HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<AlertaCliente>(b);
            b.HasIndex(x => x.FechaAviso);
            b.HasIndex(x => x.Atendida);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        modelBuilder.Entity<Presupuesto>(b =>
        {
            b.ToTable("Presupuesto");

            b.HasKey(x => x.Id);

            b.Property(x => x.NumeroPresupuesto)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Cliente)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false);

            b.Property(x => x.Matricula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Marca)
                .HasMaxLength(80)
                .IsUnicode(false);

            b.Property(x => x.Modelo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Trabajo)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Repuestos)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.ManoObra)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Estado)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");

            b.Property(x => x.Observaciones)
                .HasMaxLength(1000)
                .IsUnicode(false);

            b.Property(x => x.ConvertidoEnOrden)
                .HasDefaultValue(false);

            b.Property(x => x.Eliminado)
                .HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<Presupuesto>(b);
            b.HasIndex("WorkshopId", nameof(Presupuesto.NumeroPresupuesto)).IsUnique();
            b.HasIndex(x => x.Matricula);
            b.HasIndex(x => x.Estado);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        modelBuilder.Entity<ServicioFrecuente>(b =>
        {
            b.ToTable("ServicioFrecuente");

            b.HasKey(x => x.Id);

            b.Property(x => x.Nombre)
                .HasMaxLength(180)
                .IsUnicode(false)
                .IsRequired();

            b.Property(x => x.Eliminado)
                .HasDefaultValue(false);

            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

            ConfigureWorkshopShadow<ServicioFrecuente>(b);
            b.HasIndex("WorkshopId", nameof(ServicioFrecuente.Nombre)).IsUnique();
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static void ConfigureWorkshopShadow<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> b)
        where TEntity : class
    {
        b.Property<int>("WorkshopId")
            .HasDefaultValue(1)
            .IsRequired();

        b.HasOne<Workshop>()
            .WithMany()
            .HasForeignKey("WorkshopId")
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex("WorkshopId");
    }
}


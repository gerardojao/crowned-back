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

        // Egreso catálogo
        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
        });

        // Ingreso catálogo
        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.ToTable("Ingreso");
            entity.Property(e => e.NombreIngreso).HasMaxLength(100).IsUnicode(false);
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

            b.Property(e => e.Eliminado)
                .HasDefaultValue(false);

            // auditoría sombra
            b.Property<bool>("Activo").HasDefaultValue(true);
            b.Property<string>("UsuarioCreacion").HasMaxLength(64);
            b.Property<DateTime>("FechaCreacion");
            b.Property<string>("UsuarioModificacion").HasMaxLength(64);
            b.Property<DateTime>("FechaModificacion");

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

            b.HasIndex(e => e.Nombre);
            b.HasIndex(e => e.Categoria);
            b.HasIndex("UsuarioCreacion", "Eliminado");
        });

        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

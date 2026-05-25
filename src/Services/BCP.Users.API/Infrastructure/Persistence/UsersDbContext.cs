namespace BCP.Users.API.Infrastructure.Persistence;

using BCP.Users.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<Empresa>(e =>
        {
            e.ToTable("Empresa");
            e.HasKey(x => x.IdEmpresa);
            e.Property(x => x.IdEmpresa).ValueGeneratedNever();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.NIT).HasMaxLength(20).IsRequired();
            e.Property(x => x.RazonSocial).HasMaxLength(300).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20);
            e.HasIndex(x => x.NIT).IsUnique();
            e.Ignore(x => x.Sucursales);
        });

        modelBuilder.Entity<Sucursal>(e =>
        {
            e.ToTable("Sucursal");
            e.HasKey(x => x.IdSucursal);
            e.Property(x => x.IdSucursal).ValueGeneratedNever();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Direccion).HasMaxLength(500).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).ValueGeneratedNever();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Rol).HasMaxLength(50).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Datos semilla
        var empresaId1 = new Guid("11111111-1111-1111-1111-111111111111");
        var empresaId2 = new Guid("33333333-3333-3333-3333-333333333333");
        var sucursalId1 = new Guid("22222222-2222-2222-2222-222222222222");
        var sucursalId2 = new Guid("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Empresa>().HasData(
            new { IdEmpresa = empresaId1, Nombre = "Comercial Andina S.R.L.", NIT = "1234567890", RazonSocial = "Comercial Andina S.R.L.", Estado = "Activo", FechaRegistro = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { IdEmpresa = empresaId2, Nombre = "Importadora del Norte", NIT = "0987654321", RazonSocial = "Importadora del Norte S.A.", Estado = "Activo", FechaRegistro = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Sucursal>().HasData(
            new { IdSucursal = sucursalId1, IdEmpresa = empresaId1, Nombre = "Sucursal Centro", Direccion = "Av. 16 de Julio 1234, La Paz", Estado = "Activo" },
            new { IdSucursal = sucursalId2, IdEmpresa = empresaId1, Nombre = "Sucursal Sur", Direccion = "Av. Melchor Pérez 567, La Paz", Estado = "Activo" }
        );

        modelBuilder.Entity<Usuario>().HasData(
            new { IdUsuario = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Nombre = "Carlos Mamani", Email = "admin@bcp.com", PasswordHash = "$2a$11$placeholder_admin", Rol = "AdministradorSistema", Estado = "Activo", FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { IdUsuario = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), IdEmpresa = (Guid?)empresaId1, Nombre = "María Quispe", Email = "gerente@empresa1.com", PasswordHash = "$2a$11$placeholder_gerente", Rol = "GerenteEmpresarial", Estado = "Activo", FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { IdUsuario = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), IdEmpresa = (Guid?)empresaId1, IdSucursal = (Guid?)sucursalId1, Nombre = "Pedro Condori", Email = "supervisor@sucursal1.com", PasswordHash = "$2a$11$placeholder_supervisor", Rol = "SupervisorSucursal", Estado = "Activo", FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new { IdUsuario = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), IdEmpresa = (Guid?)empresaId1, IdSucursal = (Guid?)sucursalId1, Nombre = "Ana López", Email = "operador@sucursal1.com", PasswordHash = "$2a$11$placeholder_operador", Rol = "OperadorCaja", Estado = "Activo", FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

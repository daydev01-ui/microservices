namespace BCP.Auth.API.Infrastructure.Persistence;

using BCP.Auth.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).ValueGeneratedNever();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Rol).HasMaxLength(50).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Activo");
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Datos semilla
        var empresaId = new Guid("11111111-1111-1111-1111-111111111111");
        var sucursalId = new Guid("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Usuario>().HasData(
            new
            {
                IdUsuario = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Nombre = "Carlos Mamani",
                Email = "admin@bcp.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin2024!"),
                Rol = "AdministradorSistema",
                Estado = "Activo",
                FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                IdUsuario = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                IdEmpresa = (Guid?)empresaId,
                Nombre = "María Quispe",
                Email = "gerente@empresa1.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Gerente2024!"),
                Rol = "GerenteEmpresarial",
                Estado = "Activo",
                FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                IdUsuario = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                IdEmpresa = (Guid?)empresaId,
                IdSucursal = (Guid?)sucursalId,
                Nombre = "Pedro Condori",
                Email = "supervisor@sucursal1.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Supervisor2024!"),
                Rol = "SupervisorSucursal",
                Estado = "Activo",
                FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                IdUsuario = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                IdEmpresa = (Guid?)empresaId,
                IdSucursal = (Guid?)sucursalId,
                Nombre = "Ana López",
                Email = "operador@sucursal1.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operador2024!"),
                Rol = "OperadorCaja",
                Estado = "Activo",
                FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}

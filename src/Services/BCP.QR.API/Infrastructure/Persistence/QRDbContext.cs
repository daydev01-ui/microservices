namespace BCP.QR.API.Infrastructure.Persistence;

using BCP.QR.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class QRDbContext : DbContext
{
    public QRDbContext(DbContextOptions<QRDbContext> options) : base(options) { }
    public DbSet<QRSucursal> QRSucursales => Set<QRSucursal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("qr");
        modelBuilder.Entity<QRSucursal>(e =>
        {
            e.ToTable("QRSucursal");
            e.HasKey(x => x.IdQR);
            e.Property(x => x.IdQR).ValueGeneratedNever();
            e.Property(x => x.CodigoQR).HasMaxLength(500).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(20);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.Property(x => x.ImagenBase64).HasMaxLength(int.MaxValue);
            e.HasIndex(x => x.IdSucursal);
        });
    }
}

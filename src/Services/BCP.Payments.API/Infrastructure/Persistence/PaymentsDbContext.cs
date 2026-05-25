namespace BCP.Payments.API.Infrastructure.Persistence;

using BCP.Payments.API.Domain.Entities;
using BCP.Payments.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transactions");

        modelBuilder.Entity<Transaccion>(e =>
        {
            e.ToTable("Transaccion");
            e.HasKey(x => x.IdTransaccion);
            e.Property(x => x.IdTransaccion).ValueGeneratedNever();
            e.Property(x => x.Monto).HasPrecision(18, 2);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ReferenciaCliente).HasMaxLength(200);
            e.Property(x => x.ReferenciaExterna).HasMaxLength(200);
            e.Property(x => x.CodigoAutorizacion).HasMaxLength(100);
            e.Property(x => x.MensajeBCP).HasMaxLength(500);
            e.Property(x => x.CodigoQR).HasMaxLength(500);
            e.HasIndex(x => x.ReferenciaCliente);
            e.HasIndex(x => new { x.IdSucursal, x.FechaHora });
        });

        modelBuilder.Entity<CierreCaja>(e =>
        {
            e.ToTable("CierreCaja");
            e.HasKey(x => x.IdCierre);
            e.Property(x => x.IdCierre).ValueGeneratedNever();
            e.Property(x => x.TotalCobrado).HasPrecision(18, 2);
        });
    }
}

public class CierreCaja
{
    public Guid IdCierre { get; set; } = Guid.NewGuid();
    public Guid IdUsuario { get; set; }
    public Guid IdSucursal { get; set; }
    public decimal TotalCobrado { get; set; }
    public int TotalTransacciones { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public string Estado { get; set; } = "Abierto";
}

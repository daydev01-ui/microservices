namespace BCP.CashClosing.API.Infrastructure.Persistence;

using BCP.CashClosing.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CashClosingDbContext : DbContext
{
    public CashClosingDbContext(DbContextOptions<CashClosingDbContext> options) : base(options) { }
    public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transactions");
        modelBuilder.Entity<CierreCaja>(e =>
        {
            e.ToTable("CierreCajaTurno");
            e.HasKey(x => x.IdCierre);
            e.Property(x => x.IdCierre).ValueGeneratedNever();
            e.Property(x => x.TotalCobrado).HasPrecision(18, 2);
            e.Property(x => x.Estado).HasMaxLength(20);
            e.HasIndex(x => new { x.IdUsuario, x.Estado });
        });
    }
}

namespace BCP.Notifications.API.Infrastructure.Persistence;

using BCP.Notifications.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.Entity<Notificacion>(e =>
        {
            e.ToTable("Notificacion");
            e.HasKey(x => x.IdNotificacion);
            e.Property(x => x.Tipo).HasMaxLength(50);
            e.Property(x => x.Mensaje).HasMaxLength(500);
            e.Property(x => x.Estado).HasMaxLength(20);
        });
    }
}

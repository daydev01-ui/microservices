namespace BCP.Users.API.Domain.Events;

public record UsuarioCreadoEvent(
    Guid IdUsuario, string Nombre, string Email, string Rol,
    Guid? IdEmpresa, Guid? IdSucursal)
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}

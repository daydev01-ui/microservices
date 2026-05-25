namespace BCP.Payments.API.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OcurridoEn { get; }
}

public record PagoConfirmadoEvent(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario,
    decimal Monto, string ReferenciaExterna, DateTime FechaTransaccion) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}

public record PagoRechazadoEvent(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario,
    decimal Monto, string Motivo) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}

public record ErrorValidacionEvent(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario,
    string Mensaje) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}

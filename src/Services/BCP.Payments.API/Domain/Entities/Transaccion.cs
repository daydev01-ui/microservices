namespace BCP.Payments.API.Domain.Entities;

using BCP.Payments.API.Domain.Enums;
using BCP.Payments.API.Domain.Events;

public class Transaccion
{
    private readonly List<IDomainEvent> _eventos = new();

    public Guid IdTransaccion { get; private set; }
    public Guid IdSucursal { get; private set; }
    public Guid IdUsuario { get; private set; }
    public decimal Monto { get; private set; }
    public EstadoTransaccion Estado { get; private set; }
    public string ReferenciaCliente { get; private set; } = string.Empty;
    public string? ReferenciaExterna { get; private set; }
    public string? CodigoQR { get; private set; }
    public string? CodigoAutorizacion { get; private set; }
    public string? MensajeBCP { get; private set; }
    public int Intentos { get; private set; }
    public DateTime FechaHora { get; private set; }
    public DateTime? FechaActualizacion { get; private set; }

    public IReadOnlyList<IDomainEvent> Eventos => _eventos.AsReadOnly();

    private Transaccion() { }

    public static Transaccion Iniciar(Guid idSucursal, Guid idUsuario, decimal monto,
        string referenciaCliente, string? codigoQR = null)
    {
        if (monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero");

        return new Transaccion
        {
            IdTransaccion = Guid.NewGuid(),
            IdSucursal = idSucursal,
            IdUsuario = idUsuario,
            Monto = monto,
            ReferenciaCliente = referenciaCliente,
            CodigoQR = codigoQR,
            Estado = EstadoTransaccion.Pendiente,
            FechaHora = DateTime.UtcNow,
            Intentos = 0
        };
    }

    public void IniciarConsulta()
    {
        Estado = EstadoTransaccion.Consultando;
        Intentos++;
        FechaActualizacion = DateTime.UtcNow;
    }

    public void Confirmar(string referenciaExterna, string? codigoAutorizacion, string mensaje)
    {
        Estado = EstadoTransaccion.Confirmada;
        ReferenciaExterna = referenciaExterna;
        CodigoAutorizacion = codigoAutorizacion;
        MensajeBCP = mensaje;
        FechaActualizacion = DateTime.UtcNow;

        _eventos.Add(new PagoConfirmadoEvent(IdTransaccion, IdSucursal, IdUsuario, Monto, referenciaExterna, FechaHora));
    }

    public void Rechazar(string referenciaExterna, string mensaje)
    {
        Estado = EstadoTransaccion.Rechazada;
        ReferenciaExterna = referenciaExterna;
        MensajeBCP = mensaje;
        FechaActualizacion = DateTime.UtcNow;

        _eventos.Add(new PagoRechazadoEvent(IdTransaccion, IdSucursal, IdUsuario, Monto, mensaje));
    }

    public void RegistrarError(string mensaje)
    {
        Estado = EstadoTransaccion.Error;
        MensajeBCP = mensaje;
        FechaActualizacion = DateTime.UtcNow;

        _eventos.Add(new ErrorValidacionEvent(IdTransaccion, IdSucursal, IdUsuario, mensaje));
    }

    public bool PuedeReintentar() => Intentos < 3 && Estado == EstadoTransaccion.Consultando;

    public void LimpiarEventos() => _eventos.Clear();
}

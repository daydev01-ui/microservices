namespace BCP.Payments.API.Domain.Enums;

public enum EstadoTransaccion
{
    Pendiente = 0,
    Consultando = 1,
    Confirmada = 2,
    Rechazada = 3,
    Error = 4
}

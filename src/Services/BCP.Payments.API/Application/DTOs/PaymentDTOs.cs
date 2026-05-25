namespace BCP.Payments.API.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using BCP.Payments.API.Domain.Enums;

public class VerificarPagoRequest
{
    [Required] public Guid SucursalId { get; set; }
    [Required] public Guid UsuarioId { get; set; }
    [Required][Range(0.01, 1000000)] public decimal Monto { get; set; }
    [Required][MaxLength(200)] public string ReferenciaCliente { get; set; } = string.Empty;
    public string? CodigoQR { get; set; }
}

public class VerificarPagoResponse
{
    public Guid IdTransaccion { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ReferenciaExterna { get; set; }
    public string? CodigoAutorizacion { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public bool EsIdempotente { get; set; }
}

public class TransaccionDto
{
    public Guid IdTransaccion { get; set; }
    public Guid IdSucursal { get; set; }
    public Guid IdUsuario { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string ReferenciaCliente { get; set; } = string.Empty;
    public string? ReferenciaExterna { get; set; }
    public string? CodigoAutorizacion { get; set; }
    public string? MensajeBCP { get; set; }
    public int Intentos { get; set; }
    public DateTime FechaHora { get; set; }
}

public class ResumenPagosDto
{
    public Guid SucursalId { get; set; }
    public DateTime Fecha { get; set; }
    public int TotalTransacciones { get; set; }
    public int Confirmadas { get; set; }
    public int Rechazadas { get; set; }
    public int Pendientes { get; set; }
    public int Errores { get; set; }
    public decimal MontoTotal { get; set; }
}

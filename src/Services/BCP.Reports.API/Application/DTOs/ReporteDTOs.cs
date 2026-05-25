namespace BCP.Reports.API.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class GenerarReporteRequest
{
    [Required] public string Tipo { get; set; } = "Transacciones"; // Transacciones, CierreCaja, Conciliacion
    [Required] public string Formato { get; set; } = "Excel"; // Excel, PDF
    [Required] public DateTime PeriodoInicio { get; set; } = DateTime.Today;
    [Required] public DateTime PeriodoFin { get; set; } = DateTime.Today;
    public Guid? IdSucursal { get; set; }
    public Guid? IdEmpresa { get; set; }
    public Guid? IdUsuario { get; set; }
}

public class TransaccionReporte
{
    public Guid IdTransaccion { get; set; }
    public string ReferenciaCliente { get; set; } = string.Empty;
    public string? ReferenciaExterna { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? CodigoAutorizacion { get; set; }
    public DateTime FechaHora { get; set; }
    public Guid IdSucursal { get; set; }
    public Guid IdUsuario { get; set; }
}

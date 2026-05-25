namespace BCP.CashClosing.API.Application.DTOs;

public class AbrirCajaRequest
{
    public Guid IdUsuario { get; set; }
    public Guid IdSucursal { get; set; }
}

public class CerrarCajaRequest
{
    public Guid IdCierre { get; set; }
}

public class CierreCajaDto
{
    public Guid IdCierre { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdSucursal { get; set; }
    public decimal TotalCobrado { get; set; }
    public int TotalTransacciones { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public TimeSpan? Duracion { get; set; }
}

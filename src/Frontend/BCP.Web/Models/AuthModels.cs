namespace BCP.Web.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioInfo Usuario { get; set; } = new();
}

public class UsuarioInfo
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public Guid? IdEmpresa { get; set; }
    public Guid? IdSucursal { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

public class VerificarPagoRequest
{
    public Guid SucursalId { get; set; }
    public Guid UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string ReferenciaCliente { get; set; } = string.Empty;
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
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string ReferenciaCliente { get; set; } = string.Empty;
    public string? ReferenciaExterna { get; set; }
    public string? CodigoAutorizacion { get; set; }
    public DateTime FechaHora { get; set; }
}

public class QRDto
{
    public Guid IdQR { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
    public string? ImagenBase64 { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
}

public class ResumenDashboard
{
    public string Rol { get; set; } = string.Empty;
    public int TotalTransacciones { get; set; }
    public int TransaccionesConfirmadas { get; set; }
    public int TransaccionesRechazadas { get; set; }
    public decimal MontoTotal { get; set; }
    public int? TotalEmpresas { get; set; }
    public int? EmpresasActivas { get; set; }
    public int? TotalSucursales { get; set; }
    public int? CajasAbiertas { get; set; }
    public List<TransaccionReciente> UltimasTransacciones { get; set; } = new();
}

public class TransaccionReciente
{
    public Guid IdTransaccion { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string ReferenciaCliente { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
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
}

public class GenerarReporteRequest
{
    public string Tipo { get; set; } = "Transacciones";
    public string Formato { get; set; } = "Excel";
    public DateTime PeriodoInicio { get; set; } = DateTime.Today;
    public DateTime PeriodoFin { get; set; } = DateTime.Today;
    public Guid? IdSucursal { get; set; }
    public Guid? IdEmpresa { get; set; }
}

public class EmpresaDto
{
    public Guid IdEmpresa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int TotalSucursales { get; set; }
}

public class UsuarioDto
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Guid? IdEmpresa { get; set; }
    public Guid? IdSucursal { get; set; }
}

namespace BCP.Dashboard.API.Application.DTOs;

public class ResumenDashboard
{
    public string Rol { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Administrador del sistema
    public int? TotalEmpresas { get; set; }
    public int? EmpresasActivas { get; set; }

    // Gerente y superiores
    public int? TotalSucursales { get; set; }
    public int? SucursalesActivas { get; set; }

    // Supervisor y superiores
    public int? CajasAbiertas { get; set; }
    public int? TotalOperadores { get; set; }

    // Todos los roles con transacciones
    public int TotalTransacciones { get; set; }
    public int TransaccionesConfirmadas { get; set; }
    public int TransaccionesRechazadas { get; set; }
    public int TransaccionesPendientes { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioTransaccion { get; set; }

    public List<TransaccionReciente> UltimasTransacciones { get; set; } = new();
    public List<ActividadPorHora> ActividadPorHora { get; set; } = new();
}

public class TransaccionReciente
{
    public Guid IdTransaccion { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string ReferenciaCliente { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public Guid IdSucursal { get; set; }
}

public class ActividadPorHora
{
    public int Hora { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

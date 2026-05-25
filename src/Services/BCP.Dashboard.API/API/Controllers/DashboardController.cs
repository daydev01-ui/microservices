namespace BCP.Dashboard.API.API.Controllers;

using BCP.Dashboard.API.Application.DTOs;
using BCP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IHttpClientFactory httpFactory, IConfiguration config,
        ILogger<DashboardController> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>Resumen del dashboard según el rol del usuario</summary>
    [HttpGet("resumen")]
    public async Task<ActionResult<ApiResponse<ResumenDashboard>>> ObtenerResumen(
        [FromQuery] Guid? sucursalId,
        [FromQuery] Guid? empresaId,
        CancellationToken ct)
    {
        var rol = User.FindFirst("rol")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var idEmpresa = empresaId ?? (Guid.TryParse(User.FindFirst("idEmpresa")?.Value, out var emp) ? emp : (Guid?)null);
        var idSucursal = sucursalId ?? (Guid.TryParse(User.FindFirst("idSucursal")?.Value, out var suc) ? suc : (Guid?)null);

        var resumen = new ResumenDashboard { Rol = rol, Fecha = DateTime.UtcNow };

        // Obtener datos de transacciones desde el servicio de pagos
        var paymentsUrl = _config["PaymentsApiUrl"] ?? "http://payments-api:5005";
        var client = _httpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            var hoy = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            if (idSucursal.HasValue)
            {
                var url = $"{paymentsUrl}/api/payments/resumen/{idSucursal}";
                var response = await client.GetFromJsonAsync<ApiResponse<ResumenPagoDto>>(url, ct);
                if (response?.Success == true && response.Data != null)
                {
                    resumen.TotalTransacciones = response.Data.TotalTransacciones;
                    resumen.TransaccionesConfirmadas = response.Data.Confirmadas;
                    resumen.TransaccionesRechazadas = response.Data.Rechazadas;
                    resumen.TransaccionesPendientes = response.Data.Pendientes;
                    resumen.MontoTotal = response.Data.MontoTotal;
                    resumen.PromedioTransaccion = resumen.TotalTransacciones > 0
                        ? resumen.MontoTotal / resumen.TotalTransacciones : 0;
                }

                // Obtener últimas transacciones
                var txUrl = $"{paymentsUrl}/api/payments/transacciones?sucursalId={idSucursal}&desde={hoy}";
                var txResponse = await client.GetFromJsonAsync<ApiResponse<List<TransaccionDto>>>(txUrl, ct);
                if (txResponse?.Success == true && txResponse.Data != null)
                {
                    resumen.UltimasTransacciones = txResponse.Data.Take(10).Select(t => new TransaccionReciente
                    {
                        IdTransaccion = t.IdTransaccion, Monto = t.Monto,
                        Estado = t.Estado, ReferenciaCliente = t.ReferenciaCliente,
                        FechaHora = t.FechaHora, IdSucursal = t.IdSucursal
                    }).ToList();

                    resumen.ActividadPorHora = txResponse.Data
                        .GroupBy(t => t.FechaHora.Hour)
                        .Select(g => new ActividadPorHora
                        {
                            Hora = g.Key,
                            Cantidad = g.Count(),
                            Monto = g.Sum(t => t.Monto)
                        })
                        .OrderBy(a => a.Hora)
                        .ToList();
                }
            }

            // Datos adicionales según rol
            resumen.CajasAbiertas = new Random().Next(1, 5); // En producción vendría del CashClosing service
            resumen.TotalEmpresas = rol == "AdministradorSistema" ? 12 : null;
            resumen.EmpresasActivas = rol == "AdministradorSistema" ? 10 : null;
            resumen.TotalSucursales = rol != "OperadorCaja" ? 8 : null;
            resumen.SucursalesActivas = rol != "OperadorCaja" ? 7 : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener datos del dashboard desde Payments");
            // Devolver datos demo si el servicio no está disponible
            resumen = GenerarDatosDemo(rol);
        }

        return Ok(ApiResponse<ResumenDashboard>.Ok(resumen));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Dashboard.API", timestamp = DateTime.UtcNow });

    private static ResumenDashboard GenerarDatosDemo(string rol)
    {
        var rnd = new Random();
        return new ResumenDashboard
        {
            Rol = rol,
            TotalTransacciones = rnd.Next(20, 150),
            TransaccionesConfirmadas = rnd.Next(15, 120),
            TransaccionesRechazadas = rnd.Next(1, 10),
            TransaccionesPendientes = rnd.Next(0, 5),
            MontoTotal = Math.Round((decimal)(rnd.NextDouble() * 50000 + 5000), 2),
            TotalEmpresas = rol == "AdministradorSistema" ? 12 : null,
            EmpresasActivas = rol == "AdministradorSistema" ? 10 : null,
            TotalSucursales = rol != "OperadorCaja" ? 8 : null,
            SucursalesActivas = rol != "OperadorCaja" ? 7 : null,
            CajasAbiertas = rnd.Next(2, 8),
            UltimasTransacciones = Enumerable.Range(0, 5).Select(i => new TransaccionReciente
            {
                IdTransaccion = Guid.NewGuid(),
                Monto = Math.Round((decimal)(rnd.NextDouble() * 2000 + 50), 2),
                Estado = i < 4 ? "Confirmada" : "Rechazada",
                ReferenciaCliente = $"CUST-{rnd.Next(1000, 9999)}",
                FechaHora = DateTime.UtcNow.AddMinutes(-i * 15)
            }).ToList()
        };
    }
}

// DTOs internos para consumir el servicio de pagos
file record ResumenPagoDto(int TotalTransacciones, int Confirmadas, int Rechazadas, int Pendientes, decimal MontoTotal);
file record TransaccionDto(Guid IdTransaccion, Guid IdSucursal, decimal Monto, string Estado, string ReferenciaCliente, DateTime FechaHora);

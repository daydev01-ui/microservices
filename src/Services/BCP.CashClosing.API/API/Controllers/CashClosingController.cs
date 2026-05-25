namespace BCP.CashClosing.API.API.Controllers;

using BCP.CashClosing.API.Application.DTOs;
using BCP.CashClosing.API.Domain.Entities;
using BCP.CashClosing.API.Infrastructure.Persistence;
using BCP.Shared;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashClosingController : ControllerBase
{
    private readonly CashClosingDbContext _ctx;
    private readonly IPublishEndpoint _publisher;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<CashClosingController> _logger;

    public CashClosingController(CashClosingDbContext ctx, IPublishEndpoint publisher,
        IHttpClientFactory httpFactory, IConfiguration config, ILogger<CashClosingController> logger)
    {
        _ctx = ctx;
        _publisher = publisher;
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>Abrir caja de un operador</summary>
    [HttpPost("abrir")]
    public async Task<ActionResult<ApiResponse<CierreCajaDto>>> AbrirCaja(
        [FromBody] AbrirCajaRequest request, CancellationToken ct)
    {
        // Verificar si ya tiene caja abierta
        var cajaAbierta = await _ctx.CierresCaja
            .FirstOrDefaultAsync(c => c.IdUsuario == request.IdUsuario && c.Estado == "Abierto", ct);

        if (cajaAbierta != null)
            return Conflict(ApiResponse<CierreCajaDto>.Fail(
                "Ya existe una caja abierta para este operador"));

        var cierre = CierreCaja.Abrir(request.IdUsuario, request.IdSucursal);
        _ctx.CierresCaja.Add(cierre);
        await _ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Caja abierta: {IdCierre} usuario:{IdUsuario}", cierre.IdCierre, cierre.IdUsuario);

        return Ok(ApiResponse<CierreCajaDto>.Ok(MapearDto(cierre), "Caja abierta exitosamente"));
    }

    /// <summary>Cerrar caja y registrar totales</summary>
    [HttpPost("cerrar")]
    public async Task<ActionResult<ApiResponse<CierreCajaDto>>> CerrarCaja(
        [FromBody] CerrarCajaRequest request, CancellationToken ct)
    {
        var cierre = await _ctx.CierresCaja.FindAsync([request.IdCierre], ct);
        if (cierre is null) return NotFound(ApiResponse<CierreCajaDto>.Fail("Caja no encontrada"));
        if (!cierre.EstaAbierto()) return Conflict(ApiResponse<CierreCajaDto>.Fail("La caja ya está cerrada"));

        // Obtener totales desde el servicio de pagos
        var (totalCobrado, totalTransacciones) = await ObtenerTotalesPagosAsync(
            cierre.IdSucursal, cierre.FechaInicio, ct);

        cierre.Cerrar(totalCobrado, totalTransacciones);
        await _ctx.SaveChangesAsync(ct);

        // Publicar evento de cierre
        try
        {
            await _publisher.Publish(new CierreCajaEvent(
                cierre.IdCierre, cierre.IdSucursal, cierre.IdUsuario,
                cierre.TotalCobrado, cierre.TotalTransacciones, cierre.FechaCierre!.Value), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo publicar evento de cierre de caja");
        }

        _logger.LogInformation("Caja cerrada: {IdCierre} total:{Total}", cierre.IdCierre, cierre.TotalCobrado);
        return Ok(ApiResponse<CierreCajaDto>.Ok(MapearDto(cierre), "Caja cerrada exitosamente"));
    }

    /// <summary>Obtener caja activa de un usuario</summary>
    [HttpGet("activa/{idUsuario}")]
    public async Task<ActionResult<ApiResponse<CierreCajaDto?>>> ObtenerCajaActiva(
        Guid idUsuario, CancellationToken ct)
    {
        var cierre = await _ctx.CierresCaja
            .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario && c.Estado == "Abierto", ct);

        return Ok(ApiResponse<CierreCajaDto?>.Ok(cierre != null ? MapearDto(cierre) : null));
    }

    /// <summary>Historial de cierres de caja</summary>
    [HttpGet("historial/{idSucursal}")]
    public async Task<ActionResult<ApiResponse<List<CierreCajaDto>>>> ObtenerHistorial(
        Guid idSucursal, CancellationToken ct)
    {
        var cierres = await _ctx.CierresCaja
            .Where(c => c.IdSucursal == idSucursal)
            .OrderByDescending(c => c.FechaInicio)
            .Take(30)
            .ToListAsync(ct);

        return Ok(ApiResponse<List<CierreCajaDto>>.Ok(cierres.Select(MapearDto).ToList()));
    }

    private async Task<(decimal total, int count)> ObtenerTotalesPagosAsync(
        Guid idSucursal, DateTime desde, CancellationToken ct)
    {
        try
        {
            var paymentsUrl = _config["PaymentsApiUrl"] ?? "http://payments-api:5005";
            var client = _httpFactory.CreateClient();
            var token = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            var url = $"{paymentsUrl}/api/payments/resumen/{idSucursal}";
            var response = await client.GetFromJsonAsync<ResumenApiResponse>(url, ct);
            if (response?.Success == true && response.Data != null)
                return (response.Data.MontoTotal, response.Data.TotalTransacciones);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron obtener totales de pagos");
        }
        return (0m, 0);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.CashClosing.API", timestamp = DateTime.UtcNow });

    private static CierreCajaDto MapearDto(CierreCaja c) => new()
    {
        IdCierre = c.IdCierre, IdUsuario = c.IdUsuario, IdSucursal = c.IdSucursal,
        TotalCobrado = c.TotalCobrado, TotalTransacciones = c.TotalTransacciones,
        FechaInicio = c.FechaInicio, FechaCierre = c.FechaCierre, Estado = c.Estado,
        Duracion = c.FechaCierre.HasValue ? c.FechaCierre.Value - c.FechaInicio : DateTime.UtcNow - c.FechaInicio
    };
}

public record CierreCajaEvent(Guid IdCierre, Guid IdSucursal, Guid IdUsuario,
    decimal TotalCobrado, int TotalTransacciones, DateTime FechaCierre);

file record ResumenApiResponse(bool Success, ResumenData? Data, string Message);
file record ResumenData(int TotalTransacciones, decimal MontoTotal);

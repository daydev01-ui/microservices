namespace BCP.QR.API.API.Controllers;

using BCP.QR.API.Domain.Entities;
using BCP.QR.API.Infrastructure.Persistence;
using BCP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QRController : ControllerBase
{
    private readonly QRDbContext _ctx;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<QRController> _logger;

    public QRController(QRDbContext ctx, IHttpClientFactory httpFactory,
        IConfiguration config, ILogger<QRController> logger)
    {
        _ctx = ctx;
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>Obtener QR activo de una sucursal (genera si no existe)</summary>
    [HttpGet("{sucursalId}")]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerQR(Guid sucursalId, CancellationToken ct)
    {
        var qrExistente = await _ctx.QRSucursales
            .Where(q => q.IdSucursal == sucursalId && q.Estado == "Activo")
            .OrderByDescending(q => q.FechaEmision)
            .FirstOrDefaultAsync(ct);

        if (qrExistente != null && !qrExistente.Vencido())
        {
            return Ok(ApiResponse<object>.Ok(new
            {
                qrExistente.IdQR, qrExistente.IdSucursal,
                qrExistente.CodigoQR, qrExistente.Tipo,
                qrExistente.Estado, qrExistente.FechaEmision,
                qrExistente.FechaExpiracion, qrExistente.ImagenBase64
            }));
        }

        // Solicitar QR al stub BCP
        return await ObtenerQRDesdeBCP(sucursalId, ct);
    }

    /// <summary>Listar todos los QR de una sucursal</summary>
    [HttpGet("{sucursalId}/historial")]
    public async Task<ActionResult<ApiResponse<object>>> HistorialQR(Guid sucursalId, CancellationToken ct)
    {
        var registros = await _ctx.QRSucursales
            .Where(q => q.IdSucursal == sucursalId)
            .OrderByDescending(q => q.FechaEmision)
            .Select(q => new
            {
                q.IdQR, q.IdSucursal, q.CodigoQR, q.Tipo,
                q.Estado, q.FechaEmision, q.FechaExpiracion
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(registros));
    }

    /// <summary>Forzar actualización del QR</summary>
    [HttpPost("{sucursalId}/refresh")]
    public async Task<ActionResult<ApiResponse<object>>> RefrescarQR(Guid sucursalId, CancellationToken ct)
    {
        // Desactivar QRs anteriores
        var anteriores = await _ctx.QRSucursales
            .Where(q => q.IdSucursal == sucursalId && q.Estado == "Activo")
            .ToListAsync(ct);
        foreach (var q in anteriores) q.Desactivar();
        await _ctx.SaveChangesAsync(ct);

        return await ObtenerQRDesdeBCP(sucursalId, ct);
    }

    /// <summary>Desactivar un QR específico</summary>
    [HttpDelete("{sucursalId}/{idQR}")]
    public async Task<ActionResult<ApiResponse<object>>> DesactivarQR(
        Guid sucursalId, Guid idQR, CancellationToken ct)
    {
        var qr = await _ctx.QRSucursales
            .FirstOrDefaultAsync(q => q.IdQR == idQR && q.IdSucursal == sucursalId, ct);

        if (qr is null) return NotFound(ApiResponse<object>.Fail("QR no encontrado"));

        qr.Desactivar();
        await _ctx.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("QR desactivado exitosamente"));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.QR.API", timestamp = DateTime.UtcNow });

    private async Task<ActionResult<ApiResponse<object>>> ObtenerQRDesdeBCP(Guid sucursalId, CancellationToken ct)
    {
        try
        {
            var bcpUrl = _config["BCPStubUrl"] ?? "http://bcp-stub:5010";
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(bcpUrl);

            var response = await client.GetAsync($"api/bcp/qr/{sucursalId}", ct);
            if (!response.IsSuccessStatusCode)
                return StatusCode(502, ApiResponse<object>.Fail("Error al obtener QR del BCP"));

            var json = await response.Content.ReadAsStringAsync(ct);
            var bcpData = JsonSerializer.Deserialize<BcpQRResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (bcpData is null)
                return StatusCode(502, ApiResponse<object>.Fail("Respuesta inválida del BCP"));

            var qr = QRSucursal.Crear(sucursalId, bcpData.CodigoQR, "ESTATICO", bcpData.ImagenBase64);
            _ctx.QRSucursales.Add(qr);
            await _ctx.SaveChangesAsync(ct);

            _logger.LogInformation("QR generado para sucursal {SucursalId}: {IdQR}", sucursalId, qr.IdQR);

            return Ok(ApiResponse<object>.Ok(new
            {
                qr.IdQR, qr.IdSucursal, qr.CodigoQR, qr.Tipo,
                qr.Estado, qr.FechaEmision, qr.FechaExpiracion, qr.ImagenBase64
            }, "QR obtenido exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener QR para sucursal {SucursalId}", sucursalId);
            return StatusCode(500, ApiResponse<object>.Fail("Error al comunicarse con el BCP"));
        }
    }

    private record BcpQRResponse(string CodigoQR, string? ImagenBase64, string Estado);
}

namespace BCP.Stub.Controllers;

using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Collections.Concurrent;

[ApiController]
[Route("api/bcp")]
public class BCPStubController : ControllerBase
{
    private readonly ILogger<BCPStubController> _logger;
    // Configuración del comportamiento del stub (inyectada)
    private static readonly ConcurrentDictionary<string, string> _comportamiento = new();
    // Historial de llamadas para auditoría
    private static readonly ConcurrentQueue<LlamadaLog> _historial = new();

    public BCPStubController(ILogger<BCPStubController> logger) => _logger = logger;

    /// <summary>Obtener QR de una sucursal</summary>
    [HttpGet("qr/{sucursalId}")]
    public async Task<IActionResult> ObtenerQR(string sucursalId)
    {
        await Task.Delay(Random.Shared.Next(100, 400)); // Latencia simulada

        _logger.LogInformation("[STUB BCP] GET QR para sucursal: {SucursalId}", sucursalId);
        RegistrarLlamada("GET_QR", sucursalId);

        // Generar QR con QRCoder
        var contenidoQR = $"BCP|QR|{sucursalId}|ESTATICO|{DateTime.UtcNow:yyyyMMdd}";
        string imagenBase64;
        try
        {
            using var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrData);
            var pngBytes = qrCode.GetGraphic(5);
            imagenBase64 = $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
        }
        catch
        {
            // Fallback: SVG simple si QRCoder falla
            imagenBase64 = GenerarSVGPlaceholder(sucursalId);
        }

        return Ok(new
        {
            codigoQR = contenidoQR,
            imagenBase64,
            tipo = "ESTATICO",
            estado = "Activo",
            fechaEmision = DateTime.UtcNow,
            fechaExpiracion = DateTime.UtcNow.AddDays(365),
            sucursalId
        });
    }

    /// <summary>Verificar un pago QR</summary>
    [HttpPost("verificar-pago")]
    public async Task<IActionResult> VerificarPago([FromBody] VerificarPagoStubRequest request)
    {
        // Latencia simulada (100-800ms para simular latencia real)
        await Task.Delay(Random.Shared.Next(100, 800));

        _logger.LogInformation("[STUB BCP] POST verificar-pago: referencia={Ref} monto={Monto}",
            request.ReferenciaCliente, request.Monto);
        RegistrarLlamada("VERIFICAR_PAGO", request.ReferenciaCliente);

        // Verificar comportamiento configurado
        var comportamiento = _comportamiento.GetValueOrDefault("default", "aleatorio");
        if (_comportamiento.TryGetValue(request.ReferenciaCliente, out var comportEspecifico))
            comportamiento = comportEspecifico;

        var (estado, mensaje, codigoAutorizacion) = comportamiento switch
        {
            "confirmar" => ("Confirmado", "Pago verificado y confirmado por BCP", $"BCP-{Guid.NewGuid():N}"[..12].ToUpper()),
            "rechazar" => ("Rechazado", "Pago rechazado: fondos insuficientes", null as string),
            "timeout" => await SimularTimeout(),
            "error" => throw new Exception("Error simulado en API BCP"),
            _ => // aleatorio: 85% confirmado, 10% pendiente, 5% rechazado
                Random.Shared.Next(100) switch
                {
                    < 85 => ("Confirmado", "Pago verificado y confirmado por BCP", $"BCP-{Guid.NewGuid():N}"[..12].ToUpper()),
                    < 95 => ("Pendiente", "Pago en proceso de verificación", null as string),
                    _ => ("Rechazado", "Pago rechazado por el banco emisor", null as string)
                }
        };

        return Ok(new
        {
            referenciaExterna = Guid.NewGuid().ToString(),
            referenciaCliente = request.ReferenciaCliente,
            estado,
            monto = request.Monto,
            mensaje,
            codigoAutorizacion,
            sucursalId = request.SucursalId,
            fechaHora = DateTime.UtcNow,
            entidadBancaria = "Banco de Crédito de Bolivia S.A.",
            canal = "QR_COBRO"
        });
    }

    private static async Task<(string, string, string?)> SimularTimeout()
    {
        await Task.Delay(4000); // Supera el timeout de 3s del servicio de pagos
        return ("Timeout", "Sin respuesta del banco", null);
    }

    /// <summary>Consultar transacciones BCP de una sucursal</summary>
    [HttpGet("transacciones/{sucursalId}")]
    public async Task<IActionResult> ObtenerTransacciones(
        string sucursalId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        await Task.Delay(Random.Shared.Next(100, 300));

        var fechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.UtcNow;
        var transacciones = GenerarTransaccionesAleatorias(sucursalId, fechaDesde, fechaHasta, 15);

        return Ok(new { sucursalId, transacciones, total = transacciones.Count });
    }

    /// <summary>Configurar comportamiento del stub para pruebas</summary>
    [HttpPost("configurar")]
    public IActionResult Configurar([FromBody] ConfigurarStubRequest request)
    {
        _comportamiento[request.Clave] = request.Comportamiento;
        _logger.LogInformation("[STUB BCP] Configuración actualizada: {Clave}={Comportamiento}",
            request.Clave, request.Comportamiento);
        return Ok(new { mensaje = $"Comportamiento '{request.Comportamiento}' configurado para '{request.Clave}'" });
    }

    /// <summary>Ver historial de llamadas al stub</summary>
    [HttpGet("historial")]
    public IActionResult ObtenerHistorial()
        => Ok(new { llamadas = _historial.TakeLast(50), total = _historial.Count });

    /// <summary>Limpiar configuraciones y historial</summary>
    [HttpDelete("reset")]
    public IActionResult Reset()
    {
        _comportamiento.Clear();
        while (_historial.TryDequeue(out _)) { }
        return Ok(new { mensaje = "Stub reiniciado" });
    }

    [HttpGet("health")]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Stub", timestamp = DateTime.UtcNow });

    private static void RegistrarLlamada(string tipo, string referencia)
        => _historial.Enqueue(new LlamadaLog(tipo, referencia, DateTime.UtcNow));

    private static List<object> GenerarTransaccionesAleatorias(
        string sucursalId, DateTime desde, DateTime hasta, int cantidad)
    {
        var estados = new[] { "Confirmado", "Rechazado", "Pendiente" };
        var montos = new[] { 50m, 100m, 150m, 200m, 500m, 1000m, 1500m, 2000m, 5000m };

        return Enumerable.Range(0, cantidad).Select(i => new
        {
            referenciaExterna = Guid.NewGuid().ToString(),
            sucursalId,
            monto = montos[Random.Shared.Next(montos.Length)],
            estado = estados[Random.Shared.Next(estados.Length)],
            fechaHora = desde.AddSeconds(Random.Shared.Next(0, (int)(hasta - desde).TotalSeconds)),
            codigoAutorizacion = $"BCP-{Random.Shared.Next(100000, 999999)}"
        } as object).ToList();
    }

    private static string GenerarSVGPlaceholder(string sucursalId)
    {
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200">
              <rect width="200" height="200" fill="white" stroke="#003087" stroke-width="4"/>
              <rect x="20" y="20" width="60" height="60" fill="none" stroke="#003087" stroke-width="4"/>
              <rect x="30" y="30" width="40" height="40" fill="#003087"/>
              <rect x="120" y="20" width="60" height="60" fill="none" stroke="#003087" stroke-width="4"/>
              <rect x="130" y="30" width="40" height="40" fill="#003087"/>
              <rect x="20" y="120" width="60" height="60" fill="none" stroke="#003087" stroke-width="4"/>
              <rect x="30" y="130" width="40" height="40" fill="#003087"/>
              <text x="100" y="110" text-anchor="middle" font-family="Arial" font-size="9" fill="#003087">BCP QR</text>
              <text x="100" y="125" text-anchor="middle" font-family="Arial" font-size="7" fill="#666">{sucursalId[..Math.Min(sucursalId.Length, 20)]}</text>
            </svg>
            """;
        return $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}";
    }
}

public record VerificarPagoStubRequest(string ReferenciaCliente, decimal Monto, string SucursalId, string? CodigoQR);
public record ConfigurarStubRequest(string Clave, string Comportamiento);
public record LlamadaLog(string Tipo, string Referencia, DateTime Fecha);

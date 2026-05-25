namespace BCP.Payments.API.API.Controllers;

using BCP.Payments.API.Application.DTOs;
using BCP.Payments.API.Application.UseCases;
using BCP.Payments.API.Domain.Interfaces;
using BCP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly VerificarPagoQRUseCase _verificarPagoUseCase;
    private readonly ITransaccionRepository _transaccionRepo;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        VerificarPagoQRUseCase verificarPagoUseCase,
        ITransaccionRepository transaccionRepo,
        ILogger<PaymentsController> logger)
    {
        _verificarPagoUseCase = verificarPagoUseCase;
        _transaccionRepo = transaccionRepo;
        _logger = logger;
    }

    /// <summary>Verificar un pago QR (flujo principal)</summary>
    [HttpPost("verificar")]
    public async Task<ActionResult<ApiResponse<VerificarPagoResponse>>> Verificar(
        [FromBody] VerificarPagoRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<VerificarPagoResponse>.Fail("Datos inválidos"));

        var resultado = await _verificarPagoUseCase.EjecutarAsync(request, ct);

        var mensaje = resultado.Estado switch
        {
            "Confirmada" => "Pago verificado y confirmado exitosamente",
            "Rechazada" => "El pago fue rechazado por el banco",
            "Error" => "Error al verificar el pago. Se agotaron los reintentos",
            _ => "Pago procesado"
        };

        return Ok(ApiResponse<VerificarPagoResponse>.Ok(resultado, mensaje));
    }

    /// <summary>Listar transacciones de una sucursal</summary>
    [HttpGet("transacciones")]
    public async Task<ActionResult<ApiResponse<List<TransaccionDto>>>> ListarTransacciones(
        [FromQuery] Guid sucursalId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken ct)
    {
        var transacciones = await _transaccionRepo.ListarPorSucursalAsync(sucursalId, desde, hasta, ct);
        var dtos = transacciones.Select(t => new TransaccionDto
        {
            IdTransaccion = t.IdTransaccion,
            IdSucursal = t.IdSucursal,
            IdUsuario = t.IdUsuario,
            Monto = t.Monto,
            Estado = t.Estado.ToString(),
            ReferenciaCliente = t.ReferenciaCliente,
            ReferenciaExterna = t.ReferenciaExterna,
            CodigoAutorizacion = t.CodigoAutorizacion,
            MensajeBCP = t.MensajeBCP,
            Intentos = t.Intentos,
            FechaHora = t.FechaHora
        }).ToList();

        return Ok(ApiResponse<List<TransaccionDto>>.Ok(dtos));
    }

    /// <summary>Resumen de pagos del día por sucursal</summary>
    [HttpGet("resumen/{sucursalId}")]
    public async Task<ActionResult<ApiResponse<ResumenPagosDto>>> ObtenerResumen(Guid sucursalId, CancellationToken ct)
    {
        var hoy = DateTime.UtcNow.Date;
        var transacciones = await _transaccionRepo.ListarPorSucursalAsync(sucursalId, hoy, null, ct);
        var monto = await _transaccionRepo.SumarMontoSucursalHoyAsync(sucursalId, ct);

        var resumen = new ResumenPagosDto
        {
            SucursalId = sucursalId,
            Fecha = hoy,
            TotalTransacciones = transacciones.Count,
            Confirmadas = transacciones.Count(t => t.Estado == Domain.Enums.EstadoTransaccion.Confirmada),
            Rechazadas = transacciones.Count(t => t.Estado == Domain.Enums.EstadoTransaccion.Rechazada),
            Pendientes = transacciones.Count(t => t.Estado == Domain.Enums.EstadoTransaccion.Pendiente),
            Errores = transacciones.Count(t => t.Estado == Domain.Enums.EstadoTransaccion.Error),
            MontoTotal = monto
        };

        return Ok(ApiResponse<ResumenPagosDto>.Ok(resumen));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Payments.API", timestamp = DateTime.UtcNow });
}

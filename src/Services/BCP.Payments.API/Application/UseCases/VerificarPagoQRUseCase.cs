namespace BCP.Payments.API.Application.UseCases;

using BCP.Payments.API.Application.DTOs;
using BCP.Payments.API.Domain.Entities;
using BCP.Payments.API.Domain.Interfaces;

public class VerificarPagoQRUseCase
{
    private readonly ITransaccionRepository _transaccionRepo;
    private readonly IBCPExternalService _bcpService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<VerificarPagoQRUseCase> _logger;

    public VerificarPagoQRUseCase(
        ITransaccionRepository transaccionRepo,
        IBCPExternalService bcpService,
        IEventPublisher eventPublisher,
        ILogger<VerificarPagoQRUseCase> logger)
    {
        _transaccionRepo = transaccionRepo;
        _bcpService = bcpService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<VerificarPagoResponse> EjecutarAsync(VerificarPagoRequest request, CancellationToken ct = default)
    {
        // Regla 1: Idempotencia — si ya existe transacción con misma referencia, retornar resultado existente
        var existente = await _transaccionRepo.ObtenerPorReferenciaClienteAsync(request.ReferenciaCliente, ct);
        if (existente is not null && existente.Estado != Domain.Enums.EstadoTransaccion.Error)
        {
            _logger.LogInformation("Transacción idempotente encontrada: {Ref}", request.ReferenciaCliente);
            return MapearRespuesta(existente, esIdempotente: true);
        }

        // Crear nueva transacción
        var transaccion = Transaccion.Iniciar(
            request.SucursalId, request.UsuarioId,
            request.Monto, request.ReferenciaCliente, request.CodigoQR);

        await _transaccionRepo.GuardarAsync(transaccion, ct);
        _logger.LogInformation("Transacción iniciada: {Id} para sucursal {Sucursal}",
            transaccion.IdTransaccion, request.SucursalId);

        // Regla 2: Llamar al BCP con reintentos (hasta 3 veces, timeout 3s)
        RespuestaBCP? respuestaBCP = null;
        Exception? ultimoError = null;

        for (int intento = 1; intento <= 3; intento++)
        {
            transaccion.IniciarConsulta();
            await _transaccionRepo.ActualizarAsync(transaccion, ct);

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                respuestaBCP = await _bcpService.VerificarPagoAsync(
                    request.ReferenciaCliente, request.Monto, request.SucursalId, cts.Token);
                break; // Éxito, salir del loop
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning("Timeout en intento {Intento} para transacción {Id}", intento, transaccion.IdTransaccion);
                ultimoError = new TimeoutException($"El BCP no respondió en 3 segundos (intento {intento})");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error en intento {Intento}", intento);
                ultimoError = ex;
            }

            if (intento < 3)
                await Task.Delay(500 * intento, ct); // Backoff exponencial
        }

        // Procesar respuesta
        if (respuestaBCP is null)
        {
            var mensajeError = ultimoError?.Message ?? "Error desconocido";
            transaccion.RegistrarError(mensajeError);
            await _transaccionRepo.ActualizarAsync(transaccion, ct);

            // Publicar evento de error
            foreach (var evento in transaccion.Eventos)
                await _eventPublisher.PublicarAsync(evento, ct);

            _logger.LogError("Transacción {Id} falló después de 3 intentos: {Error}",
                transaccion.IdTransaccion, mensajeError);
            return MapearRespuesta(transaccion, esIdempotente: false);
        }

        // Aplicar resultado según respuesta BCP
        switch (respuestaBCP.Estado)
        {
            case "Confirmado":
                transaccion.Confirmar(respuestaBCP.ReferenciaExterna, respuestaBCP.CodigoAutorizacion, respuestaBCP.Mensaje);
                break;
            case "Rechazado":
                transaccion.Rechazar(respuestaBCP.ReferenciaExterna, respuestaBCP.Mensaje);
                break;
            default:
                transaccion.RegistrarError($"Estado inesperado del BCP: {respuestaBCP.Estado}");
                break;
        }

        await _transaccionRepo.ActualizarAsync(transaccion, ct);

        // Publicar domain events
        foreach (var evento in transaccion.Eventos)
            await _eventPublisher.PublicarAsync(evento, ct);

        _logger.LogInformation("Transacción {Id} procesada: {Estado}", transaccion.IdTransaccion, transaccion.Estado);
        return MapearRespuesta(transaccion, esIdempotente: false);
    }

    private static VerificarPagoResponse MapearRespuesta(Transaccion t, bool esIdempotente) => new()
    {
        IdTransaccion = t.IdTransaccion,
        Monto = t.Monto,
        Estado = t.Estado.ToString(),
        ReferenciaExterna = t.ReferenciaExterna,
        CodigoAutorizacion = t.CodigoAutorizacion,
        Mensaje = t.MensajeBCP ?? string.Empty,
        FechaHora = t.FechaHora,
        EsIdempotente = esIdempotente
    };
}

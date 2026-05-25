namespace BCP.Notifications.API.Infrastructure.Messaging;

using BCP.Notifications.API.Domain.Entities;
using BCP.Notifications.API.Hubs;
using BCP.Notifications.API.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

// Mensajes recibidos desde el bus de pagos
public record PagoConfirmadoMessage(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario,
    decimal Monto, string ReferenciaExterna, DateTime FechaTransaccion);

public record PagoRechazadoMessage(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario,
    decimal Monto, string Motivo);

public record ErrorValidacionMessage(
    Guid IdTransaccion, Guid IdSucursal, Guid IdUsuario, string Mensaje);

public record CierreCajaMessage(
    Guid IdCierre, Guid IdSucursal, Guid IdUsuario,
    decimal TotalCobrado, int TotalTransacciones, DateTime FechaCierre);

public class PagoConfirmadoConsumer : IConsumer<PagoConfirmadoMessage>
{
    private readonly NotificationsDbContext _ctx;
    private readonly IHubContext<NotificacionesHub> _hub;
    private readonly ILogger<PagoConfirmadoConsumer> _logger;

    public PagoConfirmadoConsumer(NotificationsDbContext ctx,
        IHubContext<NotificacionesHub> hub, ILogger<PagoConfirmadoConsumer> logger)
    {
        _ctx = ctx;
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PagoConfirmadoMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Pago confirmado recibido: {IdTransaccion} monto:{Monto}", msg.IdTransaccion, msg.Monto);

        var notificacion = new Notificacion
        {
            IdUsuario = msg.IdUsuario,
            IdTransaccion = msg.IdTransaccion,
            Tipo = "PagoConfirmado",
            Mensaje = $"Pago confirmado: Bs. {msg.Monto:N2} | Ref: {msg.ReferenciaExterna}",
            Estado = "Enviada"
        };
        _ctx.Notificaciones.Add(notificacion);
        await _ctx.SaveChangesAsync(context.CancellationToken);

        // Notificar a la sucursal vía SignalR
        await _hub.Clients.Group($"sucursal-{msg.IdSucursal}").SendAsync(
            "PagoConfirmado",
            new
            {
                IdTransaccion = msg.IdTransaccion,
                Monto = msg.Monto,
                ReferenciaExterna = msg.ReferenciaExterna,
                FechaHora = msg.FechaTransaccion,
                Tipo = "PagoConfirmado",
                Mensaje = notificacion.Mensaje
            },
            context.CancellationToken);

        // También notificar a administradores
        await _hub.Clients.Group("administradores").SendAsync(
            "NuevaTransaccion",
            new { msg.IdTransaccion, msg.Monto, msg.IdSucursal, Estado = "Confirmada" },
            context.CancellationToken);
    }
}

public class PagoRechazadoConsumer : IConsumer<PagoRechazadoMessage>
{
    private readonly NotificationsDbContext _ctx;
    private readonly IHubContext<NotificacionesHub> _hub;

    public PagoRechazadoConsumer(NotificationsDbContext ctx, IHubContext<NotificacionesHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<PagoRechazadoMessage> context)
    {
        var msg = context.Message;
        var notificacion = new Notificacion
        {
            IdUsuario = msg.IdUsuario,
            IdTransaccion = msg.IdTransaccion,
            Tipo = "PagoRechazado",
            Mensaje = $"Pago rechazado: Bs. {msg.Monto:N2} | Motivo: {msg.Motivo}",
            Estado = "Enviada"
        };
        _ctx.Notificaciones.Add(notificacion);
        await _ctx.SaveChangesAsync(context.CancellationToken);

        await _hub.Clients.Group($"sucursal-{msg.IdSucursal}").SendAsync(
            "PagoRechazado",
            new { msg.IdTransaccion, msg.Monto, msg.Motivo, Tipo = "PagoRechazado" },
            context.CancellationToken);
    }
}

public class CierreCajaConsumer : IConsumer<CierreCajaMessage>
{
    private readonly NotificationsDbContext _ctx;
    private readonly IHubContext<NotificacionesHub> _hub;

    public CierreCajaConsumer(NotificationsDbContext ctx, IHubContext<NotificacionesHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    public async Task Consume(ConsumeContext<CierreCajaMessage> context)
    {
        var msg = context.Message;
        var notificacion = new Notificacion
        {
            IdUsuario = msg.IdUsuario,
            Tipo = "CierreCaja",
            Mensaje = $"Caja cerrada: {msg.TotalTransacciones} transacciones | Total: Bs. {msg.TotalCobrado:N2}",
            Estado = "Enviada"
        };
        _ctx.Notificaciones.Add(notificacion);
        await _ctx.SaveChangesAsync(context.CancellationToken);

        await _hub.Clients.Group($"sucursal-{msg.IdSucursal}").SendAsync(
            "CierreCaja",
            new { msg.IdCierre, msg.TotalCobrado, msg.TotalTransacciones, msg.FechaCierre },
            context.CancellationToken);
    }
}

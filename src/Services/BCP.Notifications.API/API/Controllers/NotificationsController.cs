namespace BCP.Notifications.API.API.Controllers;

using BCP.Notifications.API.Infrastructure.Persistence;
using BCP.Notifications.API.Hubs;
using BCP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationsDbContext _ctx;
    private readonly IHubContext<NotificacionesHub> _hub;

    public NotificationsController(NotificationsDbContext ctx, IHubContext<NotificacionesHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> ObtenerNotificaciones(CancellationToken ct)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        var notifs = await _ctx.Notificaciones
            .Where(n => n.IdUsuario == userId)
            .OrderByDescending(n => n.FechaEnvio)
            .Take(50)
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { notificaciones = notifs, noLeidas = notifs.Count(n => !n.Leida) }));
    }

    [HttpPut("{id}/leer")]
    public async Task<IActionResult> MarcarLeida(Guid id, CancellationToken ct)
    {
        var notif = await _ctx.Notificaciones.FindAsync([id], ct);
        if (notif is null) return NotFound();
        notif.Leida = true;
        await _ctx.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("Notificación marcada como leída"));
    }

    [HttpPost("test")]
    public async Task<IActionResult> EnviarPrueba([FromBody] TestNotifRequest req, CancellationToken ct)
    {
        await _hub.Clients.All.SendAsync("PagoConfirmado", new
        {
            IdTransaccion = Guid.NewGuid(),
            Monto = req.Monto,
            ReferenciaExterna = "TEST-001",
            FechaHora = DateTime.UtcNow,
            Tipo = "PagoConfirmado",
            Mensaje = $"[PRUEBA] Pago confirmado: Bs. {req.Monto:N2}"
        }, ct);
        return Ok(ApiResponse.Ok("Notificación de prueba enviada"));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Notifications.API", timestamp = DateTime.UtcNow });
}

public record TestNotifRequest(decimal Monto = 100);

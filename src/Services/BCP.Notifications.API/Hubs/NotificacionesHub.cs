namespace BCP.Notifications.API.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class NotificacionesHub : Hub
{
    private readonly ILogger<NotificacionesHub> _logger;

    public NotificacionesHub(ILogger<NotificacionesHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var rol = Context.User?.FindFirst("rol")?.Value;
        var idEmpresa = Context.User?.FindFirst("idEmpresa")?.Value;
        var idSucursal = Context.User?.FindFirst("idSucursal")?.Value;

        _logger.LogInformation("Cliente conectado: {UserId} rol:{Rol}", userId, rol);

        // Agregar a grupos según contexto
        if (!string.IsNullOrEmpty(idSucursal))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"sucursal-{idSucursal}");
        if (!string.IsNullOrEmpty(idEmpresa))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"empresa-{idEmpresa}");
        if (rol == "AdministradorSistema")
            await Groups.AddToGroupAsync(Context.ConnectionId, "administradores");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

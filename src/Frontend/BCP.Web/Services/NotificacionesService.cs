namespace BCP.Web.Services;

using Microsoft.AspNetCore.SignalR.Client;

public class NotificacionesService : IAsyncDisposable
{
    private HubConnection? _hub;
    private readonly IConfiguration _config;
    private readonly AuthService _authService;
    private readonly ILogger<NotificacionesService> _logger;

    public event Action<string, string>? OnNotificacion;

    public NotificacionesService(IConfiguration config, AuthService authService, ILogger<NotificacionesService> logger)
    {
        _config = config;
        _authService = authService;
        _logger = logger;
    }

    public async Task ConectarAsync()
    {
        var token = await _authService.ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token)) return;

        var notifUrl = _config["NotificationsUrl"] ?? "http://localhost:5008";

        _hub = new HubConnectionBuilder()
            .WithUrl($"{notifUrl}/notificaciones-hub", opts =>
            {
                opts.AccessTokenProvider = async () => await _authService.ObtenerTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.On<string, string>("RecibirNotificacion", (tipo, mensaje) =>
        {
            OnNotificacion?.Invoke(tipo, mensaje);
        });

        try
        {
            await _hub.StartAsync();
            _logger.LogInformation("SignalR conectado");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar SignalR (notificaciones desactivadas)");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null)
            await _hub.DisposeAsync();
    }
}

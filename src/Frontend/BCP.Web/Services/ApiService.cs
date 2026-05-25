namespace BCP.Web.Services;

using System.Net.Http.Headers;
using BCP.Web.Models;

public class ApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly AuthService _authService;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory factory, AuthService authService, ILogger<ApiService> logger)
    {
        _factory = factory;
        _authService = authService;
        _logger = logger;
    }

    private async Task<HttpClient> GetClientAsync()
    {
        var client = _factory.CreateClient("BcpApi");
        var token = await _authService.ObtenerTokenAsync();
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<HttpClient> GetFileClientAsync()
    {
        var client = _factory.CreateClient("BcpApiFileDownload");
        var token = await _authService.ObtenerTokenAsync();
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ─── DASHBOARD ───────────────────────────────────────────────

    public async Task<ResumenDashboard?> ObtenerResumenDashboardAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var usuario = await _authService.ObtenerUsuarioAsync();
            var query = "";
            if (usuario?.IdSucursal.HasValue == true) query = $"?sucursalId={usuario.IdSucursal}";
            else if (usuario?.IdEmpresa.HasValue == true) query = $"?empresaId={usuario.IdEmpresa}";

            var resp = await client.GetFromJsonAsync<ApiResponse<ResumenDashboard>>($"api/dashboard/resumen{query}");
            return resp?.Data;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo dashboard"); return null; }
    }

    // ─── PAGOS ───────────────────────────────────────────────────

    public async Task<(bool ok, VerificarPagoResponse? data, string msg)> VerificarPagoAsync(VerificarPagoRequest req)
    {
        try
        {
            var client = await GetClientAsync();
            var resp = await client.PostAsJsonAsync("api/payments/verificar", req);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<VerificarPagoResponse>>();
            return (result?.Success == true, result?.Data, result?.Message ?? "Error desconocido");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error verificando pago"); return (false, null, ex.Message); }
    }

    public async Task<List<TransaccionDto>> ObtenerTransaccionesAsync(Guid? sucursalId = null)
    {
        try
        {
            var client = await GetClientAsync();
            if (!sucursalId.HasValue)
            {
                var u = await _authService.ObtenerUsuarioAsync();
                sucursalId = u?.IdSucursal;
            }
            var url = sucursalId.HasValue
                ? $"api/payments/transacciones?sucursalId={sucursalId}"
                : "api/payments/transacciones";
            var resp = await client.GetFromJsonAsync<ApiResponse<List<TransaccionDto>>>(url);
            return resp?.Data ?? new();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo transacciones"); return new(); }
    }

    // ─── QR ──────────────────────────────────────────────────────

    public async Task<(bool ok, QRDto? data, string msg)> GenerarQRAsync(Guid sucursalId)
    {
        try
        {
            var client = await GetClientAsync();
            // Refresh fuerza la generación de nuevo QR
            var resp = await client.PostAsync($"api/qr/{sucursalId}/refresh", null);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<QRDto>>();
            return (result?.Success == true, result?.Data, result?.Message ?? "Error");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error generando QR"); return (false, null, ex.Message); }
    }

    public async Task<QRDto?> ObtenerQRActivoAsync(Guid sucursalId)
    {
        try
        {
            var client = await GetClientAsync();
            var resp = await client.GetFromJsonAsync<ApiResponse<QRDto>>($"api/qr/{sucursalId}");
            return resp?.Data;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo QR"); return null; }
    }

    public async Task<List<QRDto>> ObtenerQRsAsync(Guid? sucursalId = null)
    {
        try
        {
            var client = await GetClientAsync();
            if (!sucursalId.HasValue)
            {
                var u = await _authService.ObtenerUsuarioAsync();
                sucursalId = u?.IdSucursal;
            }
            if (!sucursalId.HasValue) return new();

            var resp = await client.GetFromJsonAsync<ApiResponse<List<QRDto>>>($"api/qr/{sucursalId}/historial");
            return resp?.Data ?? new();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo QRs"); return new(); }
    }

    // ─── USUARIOS ────────────────────────────────────────────────

    public async Task<List<UsuarioDto>> ObtenerUsuariosAsync(Guid? idEmpresa = null)
    {
        try
        {
            var client = await GetClientAsync();
            var url = idEmpresa.HasValue
                ? $"api/users/usuarios?idEmpresa={idEmpresa}"
                : "api/users/usuarios";
            var resp = await client.GetFromJsonAsync<ApiResponse<List<UsuarioDto>>>(url);
            return resp?.Data ?? new();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo usuarios"); return new(); }
    }

    public async Task<List<EmpresaDto>> ObtenerEmpresasAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var resp = await client.GetFromJsonAsync<ApiResponse<List<EmpresaDto>>>("api/users/empresas");
            return resp?.Data ?? new();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo empresas"); return new(); }
    }

    // ─── REPORTES ────────────────────────────────────────────────

    public async Task<(bool ok, byte[]? data, string msg)> GenerarReporteAsync(GenerarReporteRequest req)
    {
        try
        {
            var client = await GetFileClientAsync();
            var resp = await client.PostAsJsonAsync("api/reports/generar", req);
            if (resp.IsSuccessStatusCode)
            {
                var bytes = await resp.Content.ReadAsByteArrayAsync();
                return (true, bytes, "Reporte generado exitosamente");
            }
            return (false, null, "Error generando reporte");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error generando reporte"); return (false, null, ex.Message); }
    }

    // ─── CIERRE DE CAJA ──────────────────────────────────────────

    public async Task<(bool ok, CierreCajaDto? data, string msg)> AbrirCajaAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var usuario = await _authService.ObtenerUsuarioAsync();
            var req = new
            {
                IdUsuario = usuario?.IdUsuario ?? Guid.Empty,
                IdSucursal = usuario?.IdSucursal ?? Guid.Empty
            };
            var resp = await client.PostAsJsonAsync("api/cashclosing/abrir", req);
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<CierreCajaDto>>();
            return (result?.Success == true, result?.Data, result?.Message ?? "Error");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error abriendo caja"); return (false, null, ex.Message); }
    }

    public async Task<(bool ok, CierreCajaDto? data, string msg)> CerrarCajaAsync(Guid idCierre)
    {
        try
        {
            var client = await GetClientAsync();
            var resp = await client.PostAsJsonAsync("api/cashclosing/cerrar", new { IdCierre = idCierre });
            var result = await resp.Content.ReadFromJsonAsync<ApiResponse<CierreCajaDto>>();
            return (result?.Success == true, result?.Data, result?.Message ?? "Error");
        }
        catch (Exception ex) { _logger.LogError(ex, "Error cerrando caja"); return (false, null, ex.Message); }
    }

    public async Task<CierreCajaDto?> ObtenerCajaActivaAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var usuario = await _authService.ObtenerUsuarioAsync();
            if (usuario == null) return null;
            var resp = await client.GetFromJsonAsync<ApiResponse<CierreCajaDto>>($"api/cashclosing/activa/{usuario.IdUsuario}");
            return resp?.Data;
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo caja activa"); return null; }
    }

    public async Task<List<CierreCajaDto>> ObtenerHistorialCajasAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var usuario = await _authService.ObtenerUsuarioAsync();
            var sucursalId = usuario?.IdSucursal ?? Guid.Empty;
            if (sucursalId == Guid.Empty) return new();
            var resp = await client.GetFromJsonAsync<ApiResponse<List<CierreCajaDto>>>($"api/cashclosing/historial/{sucursalId}");
            return resp?.Data ?? new();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error obteniendo historial"); return new(); }
    }
}

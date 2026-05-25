namespace BCP.Web.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCP.Web.Models;
using Blazored.LocalStorage;

public class AuthService
{
    private readonly IHttpClientFactory _factory;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;

    private const string TokenKey = "bcp_token";
    private const string UserKey = "bcp_user";

    public AuthService(IHttpClientFactory factory, ILocalStorageService localStorage,
        ILogger<AuthService> logger)
    {
        _factory = factory;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<(bool success, string message)> LoginAsync(LoginRequest request)
    {
        try
        {
            var client = _factory.CreateClient("BcpApi");
            var response = await client.PostAsJsonAsync("api/auth/login", request);
            var apiResp = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

            if (apiResp?.Success == true && apiResp.Data != null)
            {
                await _localStorage.SetItemAsync(TokenKey, apiResp.Data.Token);
                await _localStorage.SetItemAsync(UserKey, apiResp.Data.Usuario);
                return (true, "Inicio de sesión exitoso");
            }

            return (false, apiResp?.Message ?? "Credenciales incorrectas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login");
            return (false, "Error de conexión. Verifique que el servidor esté disponible.");
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserKey);
    }

    public async Task<string?> ObtenerTokenAsync()
        => await _localStorage.GetItemAsync<string?>(TokenKey);

    public async Task<UsuarioInfo?> ObtenerUsuarioAsync()
        => await _localStorage.GetItemAsync<UsuarioInfo?>(UserKey);

    public async Task<bool> EstaAutenticadoAsync()
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token)) return false;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch { return false; }
    }

    public async Task<ClaimsPrincipal> ObtenerClaimsAsync()
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token)) return new ClaimsPrincipal();
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow) return new ClaimsPrincipal();
            var identity = new ClaimsIdentity(jwt.Claims, "BcpJwt");
            return new ClaimsPrincipal(identity);
        }
        catch { return new ClaimsPrincipal(); }
    }
}

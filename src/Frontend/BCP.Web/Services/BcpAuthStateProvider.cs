namespace BCP.Web.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

public class BcpAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public BcpAuthStateProvider(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = await _authService.ObtenerClaimsAsync();
        return new AuthenticationState(principal);
    }

    public void NotificarCambioEstado()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

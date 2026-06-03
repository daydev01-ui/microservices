using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthServiceLogic _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthServiceLogic authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Email}", request.CorreoElectronico);
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { mensaje = "Credenciales inválidas" });
        return Ok(result);
    }
}

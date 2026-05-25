namespace BCP.Auth.API.API.Controllers;

using BCP.Auth.API.Application.DTOs;
using BCP.Auth.API.Application.UseCases;
using BCP.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly ILogger<AuthController> _logger;

    public AuthController(LoginUseCase loginUseCase, ILogger<AuthController> logger)
    {
        _loginUseCase = loginUseCase;
        _logger = logger;
    }

    /// <summary>Autenticar usuario y obtener JWT</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LoginResponse>.Fail("Datos inválidos",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

        var result = await _loginUseCase.EjecutarAsync(request, ct);

        if (result is null)
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Credenciales incorrectas"));

        return Ok(ApiResponse<LoginResponse>.Ok(result, "Inicio de sesión exitoso"));
    }

    /// <summary>Verificar validez del token</summary>
    [HttpGet("verify")]
    [Authorize]
    public IActionResult Verify()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var rol = User.FindFirst("rol")?.Value;
        return Ok(ApiResponse<object>.Ok(new { userId, rol }, "Token válido"));
    }

    /// <summary>Cerrar sesión (cliente elimina el token)</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
        => Ok(ApiResponse.Ok("Sesión cerrada exitosamente"));

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Auth.API", timestamp = DateTime.UtcNow });
}

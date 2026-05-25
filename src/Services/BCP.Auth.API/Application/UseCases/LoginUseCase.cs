namespace BCP.Auth.API.Application.UseCases;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCP.Auth.API.Application.DTOs;
using BCP.Auth.API.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

public class LoginUseCase
{
    private readonly IUsuarioRepository _repo;
    private readonly IConfiguration _config;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(IUsuarioRepository repo, IConfiguration config, ILogger<LoginUseCase> logger)
    {
        _repo = repo;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResponse?> EjecutarAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(request.Email, ct);

        if (usuario is null || !usuario.EstaActivo())
        {
            _logger.LogWarning("Intento de login fallido para email: {Email}", request.Email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Contraseña incorrecta para: {Email}", request.Email);
            return null;
        }

        usuario.RegistrarAcceso();
        await _repo.ActualizarAsync(usuario, ct);

        var (token, expiracion) = GenerarJwt(usuario);
        var refreshToken = GenerarRefreshToken();

        _logger.LogInformation("Login exitoso para usuario: {Email} rol: {Rol}", usuario.Email, usuario.Rol);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiracion = expiracion,
            Usuario = new UsuarioInfo
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                IdEmpresa = usuario.IdEmpresa,
                IdSucursal = usuario.IdSucursal
            }
        };
    }

    private (string token, DateTime expiracion) GenerarJwt(Domain.Entities.Usuario usuario)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurado");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracion = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Role, usuario.Rol),
            new("rol", usuario.Rol),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (usuario.IdEmpresa.HasValue)
            claims.Add(new("idEmpresa", usuario.IdEmpresa.Value.ToString()));
        if (usuario.IdSucursal.HasValue)
            claims.Add(new("idSucursal", usuario.IdSucursal.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }

    private static string GenerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

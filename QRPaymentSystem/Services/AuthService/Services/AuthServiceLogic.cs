using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using AuthService.Models;

namespace AuthService.Services;

public interface IAuthServiceLogic
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthServiceLogic : IAuthServiceLogic
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceLogic> _logger;

    public AuthServiceLogic(IConfiguration configuration, ILogger<AuthServiceLogic> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Email}", request.CorreoElectronico);

        await using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var user = await conn.QueryFirstOrDefaultAsync<Usuario>(
            @"SELECT u.IdUsuario, u.NombreCompleto, u.CorreoElectronico, u.Contraseña,
                     u.EstadoUsuario, u.IdRol, r.NombreRol, u.IdEmpresa, e.NombreEmpresa
              FROM Usuarios u
              INNER JOIN Roles r ON u.IdRol = r.IdRol
              INNER JOIN Empresas e ON u.IdEmpresa = e.IdEmpresa
              WHERE u.CorreoElectronico = @Email AND u.EstadoUsuario = 'Activo'",
            new { Email = request.CorreoElectronico });

        if (user == null)
        {
            _logger.LogWarning("User not found: {Email}", request.CorreoElectronico);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Contraseña, user.Contraseña))
        {
            _logger.LogWarning("Invalid password for: {Email}", request.CorreoElectronico);
            return null;
        }

        var token = GenerateJwtToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60"));

        return new LoginResponse
        {
            Token = token,
            NombreCompleto = user.NombreCompleto,
            CorreoElectronico = user.CorreoElectronico,
            Rol = user.NombreRol ?? string.Empty,
            Expiration = expiration
        };
    }

    private string GenerateJwtToken(Usuario user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.CorreoElectronico),
            new Claim(ClaimTypes.Name, user.NombreCompleto),
            new Claim(ClaimTypes.Role, user.NombreRol ?? string.Empty),
            new Claim("empresaId", user.IdEmpresa.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["ExpirationMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

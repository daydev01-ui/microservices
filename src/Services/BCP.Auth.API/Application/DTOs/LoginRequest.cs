namespace BCP.Auth.API.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioInfo Usuario { get; set; } = new();
}

public class UsuarioInfo
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public Guid? IdEmpresa { get; set; }
    public Guid? IdSucursal { get; set; }
}

public class RefreshTokenRequest
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}

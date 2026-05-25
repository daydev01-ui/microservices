namespace BCP.Users.API.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public class CrearEmpresaRequest
{
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Required][MaxLength(20)] public string NIT { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string RazonSocial { get; set; } = string.Empty;
}

public class ActualizarEmpresaRequest
{
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string RazonSocial { get; set; } = string.Empty;
}

public class CrearSucursalRequest
{
    [Required] public Guid IdEmpresa { get; set; }
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Required][MaxLength(500)] public string Direccion { get; set; } = string.Empty;
}

public class ActualizarSucursalRequest
{
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Required][MaxLength(500)] public string Direccion { get; set; } = string.Empty;
}

public class CrearUsuarioRequest
{
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required][MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string Rol { get; set; } = string.Empty;
    public Guid? IdEmpresa { get; set; }
    public Guid? IdSucursal { get; set; }
}

public class ActualizarUsuarioRequest
{
    [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public Guid? IdSucursal { get; set; }
}

public class EmpresaDto
{
    public Guid IdEmpresa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int TotalSucursales { get; set; }
}

public class SucursalDto
{
    public Guid IdSucursal { get; set; }
    public Guid IdEmpresa { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public Guid IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public Guid? IdEmpresa { get; set; }
    public Guid? IdSucursal { get; set; }
    public DateTime FechaCreacion { get; set; }
}

namespace AuthService.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public string EstadoUsuario { get; set; } = "Activo";
    public int IdRol { get; set; }
    public string? NombreRol { get; set; }
    public int IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}

public class Rol
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = string.Empty;
}

// Alias for compatibility
public class Role : Rol { }

public class Empresa
{
    public int IdEmpresa { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? NIT { get; set; }
    public string? Direccion { get; set; }
    public string EstadoEmpresa { get; set; } = "Activo";
}

public class LoginRequest
{
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}

public class CreateUserRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public int IdRol { get; set; }
    public int IdEmpresa { get; set; }
}

public class UpdateUserRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string EstadoUsuario { get; set; } = "Activo";
    public int IdRol { get; set; }
    public int IdEmpresa { get; set; }
}

public class AssignRoleRequest
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
}

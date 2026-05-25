namespace BCP.Auth.API.Domain.Entities;

public class Usuario
{
    public Guid IdUsuario { get; private set; }
    public Guid? IdEmpresa { get; private set; }
    public Guid? IdSucursal { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Rol { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Activo";
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; private set; }

    private Usuario() { }

    public static Usuario Crear(string nombre, string email, string passwordHash, string rol,
        Guid? idEmpresa = null, Guid? idSucursal = null)
    {
        return new Usuario
        {
            IdUsuario = Guid.NewGuid(),
            Nombre = nombre,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Rol = rol,
            IdEmpresa = idEmpresa,
            IdSucursal = idSucursal,
            Estado = "Activo",
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void RegistrarAcceso() => UltimoAcceso = DateTime.UtcNow;

    public bool EstaActivo() => Estado == "Activo";
}

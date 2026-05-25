namespace BCP.Users.API.Domain.Entities;

public class Sucursal
{
    public Guid IdSucursal { get; private set; }
    public Guid IdEmpresa { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Activo";

    private Sucursal() { }

    public static Sucursal Crear(Guid idEmpresa, string nombre, string direccion)
    {
        return new Sucursal
        {
            IdSucursal = Guid.NewGuid(),
            IdEmpresa = idEmpresa,
            Nombre = nombre.Trim(),
            Direccion = direccion.Trim(),
            Estado = "Activo"
        };
    }

    public void Actualizar(string nombre, string direccion)
    {
        Nombre = nombre.Trim();
        Direccion = direccion.Trim();
    }

    public void Desactivar() => Estado = "Inactivo";
    public bool EstaActiva() => Estado == "Activo";
}

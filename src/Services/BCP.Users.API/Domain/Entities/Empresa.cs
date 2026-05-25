namespace BCP.Users.API.Domain.Entities;

public class Empresa
{
    public Guid IdEmpresa { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string NIT { get; private set; } = string.Empty;
    public string RazonSocial { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "Activo";
    public DateTime FechaRegistro { get; private set; } = DateTime.UtcNow;

    private readonly List<Sucursal> _sucursales = new();
    public IReadOnlyList<Sucursal> Sucursales => _sucursales.AsReadOnly();

    private Empresa() { }

    public static Empresa Crear(string nombre, string nit, string razonSocial)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido");
        if (string.IsNullOrWhiteSpace(nit)) throw new ArgumentException("NIT requerido");

        return new Empresa
        {
            IdEmpresa = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            NIT = nit.Trim(),
            RazonSocial = razonSocial.Trim(),
            Estado = "Activo",
            FechaRegistro = DateTime.UtcNow
        };
    }

    public void Actualizar(string nombre, string razonSocial)
    {
        Nombre = nombre.Trim();
        RazonSocial = razonSocial.Trim();
    }

    public void Desactivar() => Estado = "Inactivo";
    public void Activar() => Estado = "Activo";
    public bool EstaActiva() => Estado == "Activo";
}

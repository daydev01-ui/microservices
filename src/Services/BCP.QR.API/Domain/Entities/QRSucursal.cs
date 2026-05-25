namespace BCP.QR.API.Domain.Entities;

public class QRSucursal
{
    public Guid IdQR { get; private set; }
    public Guid IdSucursal { get; private set; }
    public string CodigoQR { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = "ESTATICO";
    public string Estado { get; private set; } = "Activo";
    public DateTime FechaEmision { get; private set; }
    public DateTime? FechaExpiracion { get; private set; }
    public string? ImagenBase64 { get; private set; }

    private QRSucursal() { }

    public static QRSucursal Crear(Guid idSucursal, string codigoQR, string tipo = "ESTATICO", string? imagenBase64 = null)
    {
        return new QRSucursal
        {
            IdQR = Guid.NewGuid(),
            IdSucursal = idSucursal,
            CodigoQR = codigoQR,
            Tipo = tipo,
            Estado = "Activo",
            FechaEmision = DateTime.UtcNow,
            FechaExpiracion = tipo == "ESTATICO" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddHours(24),
            ImagenBase64 = imagenBase64
        };
    }

    public void ActualizarImagen(string imagenBase64) => ImagenBase64 = imagenBase64;
    public void Desactivar() => Estado = "Inactivo";
    public bool EstaActivo() => Estado == "Activo";
    public bool Vencido() => FechaExpiracion.HasValue && FechaExpiracion.Value < DateTime.UtcNow;
}

namespace BCP.CashClosing.API.Domain.Entities;

public class CierreCaja
{
    public Guid IdCierre { get; private set; }
    public Guid IdUsuario { get; private set; }
    public Guid IdSucursal { get; private set; }
    public decimal TotalCobrado { get; private set; }
    public int TotalTransacciones { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaCierre { get; private set; }
    public string Estado { get; private set; } = "Abierto";

    private CierreCaja() { }

    public static CierreCaja Abrir(Guid idUsuario, Guid idSucursal)
    {
        return new CierreCaja
        {
            IdCierre = Guid.NewGuid(),
            IdUsuario = idUsuario,
            IdSucursal = idSucursal,
            FechaInicio = DateTime.UtcNow,
            Estado = "Abierto"
        };
    }

    public void Cerrar(decimal totalCobrado, int totalTransacciones)
    {
        if (Estado != "Abierto")
            throw new InvalidOperationException("Solo se puede cerrar una caja abierta");

        TotalCobrado = totalCobrado;
        TotalTransacciones = totalTransacciones;
        FechaCierre = DateTime.UtcNow;
        Estado = "Cerrado";
    }

    public bool EstaAbierto() => Estado == "Abierto";
}

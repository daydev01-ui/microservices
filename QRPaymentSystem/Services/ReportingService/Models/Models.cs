namespace ReportingService.Models;

public class ReportSummary
{
    public int TotalTransacciones { get; set; }
    public int TotalValidadas { get; set; }
    public int TotalRechazadas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal TasaExito { get; set; }
    public IEnumerable<TransaccionReporte> Transacciones { get; set; } = [];
}

public class TransaccionReporte
{
    public int IdTransaccion { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string EstadoTransaccion { get; set; } = string.Empty;
    public string CodigoQR { get; set; } = string.Empty;
    public decimal? Monto { get; set; }
    public string? NombreEmpresa { get; set; }
    public decimal? MontoPago { get; set; }
}

public class RealTimeStats
{
    public int TransaccionesHoy { get; set; }
    public int ValidadasHoy { get; set; }
    public int RechazadasHoy { get; set; }
    public decimal MontoHoy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

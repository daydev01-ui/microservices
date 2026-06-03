namespace QRService.Models;

public class CodigoQR
{
    public int IdQR { get; set; }
    public string CodigoQR_ { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string EstadoQR { get; set; } = "Activo";
    public int IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}

public class GenerateQRRequest
{
    public int EmpresaId { get; set; }
    public decimal Monto { get; set; }
}

public class GenerateQRResponse
{
    public int IdQR { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string QRImageBase64 { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string EstadoQR { get; set; } = "Activo";
}

public class UpdateStatusRequest
{
    public string Estado { get; set; } = string.Empty;
}

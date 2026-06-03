namespace TransactionService.Models;

public class Transaccion
{
    public int IdTransaccion { get; set; }
    public DateTime FechaTransaccion { get; set; }
    public string EstadoTransaccion { get; set; } = string.Empty;
    public int IdQR { get; set; }
    public string? CodigoQR { get; set; }
    public decimal? Monto { get; set; }
    public string? NombreEmpresa { get; set; }
}

public class Pago
{
    public int IdPago { get; set; }
    public decimal MontoPago { get; set; }
    public DateTime FechaPago { get; set; }
    public string MetodoPago { get; set; } = "QR";
    public int IdTransaccion { get; set; }
}

public class TransaccionDetalle : Transaccion
{
    public Pago? Pago { get; set; }
}

public class ValidatePaymentRequest
{
    public string CodigoQr { get; set; } = string.Empty;
}

public class ValidatePaymentResponse
{
    public int? IdTransaccion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public decimal? Monto { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class TransactionFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Estado { get; set; }
    public decimal? MinMonto { get; set; }
    public decimal? MaxMonto { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}

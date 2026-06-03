using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using TransactionService.Models;

namespace TransactionService.Services;

public class TransactionServiceImpl
{
    private readonly string _connStr;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TransactionServiceImpl> _logger;

    public TransactionServiceImpl(IConfiguration config, IHttpClientFactory factory, ILogger<TransactionServiceImpl> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")!;
        _httpClient = factory.CreateClient("BcpMock");
        _logger = logger;
    }

    public async Task<ValidatePaymentResponse> ValidatePaymentAsync(ValidatePaymentRequest req)
    {
        using var conn = new SqlConnection(_connStr);

        // Get QR info
        var qr = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT IdQR, Monto, EstadoQR FROM CodigosQR WHERE CodigoQR = @Codigo",
            new { Codigo = req.CodigoQr });

        if (qr == null)
            return new ValidatePaymentResponse { Estado = "Error", Mensaje = "Código QR no encontrado" };

        if (qr.EstadoQR != "Activo")
            return new ValidatePaymentResponse { Estado = "Error", Mensaje = "Código QR no está activo" };

        // Idempotency check
        var existing = await conn.QueryFirstOrDefaultAsync<Transaccion>(
            "SELECT * FROM Transacciones WHERE IdQR = @IdQR AND EstadoTransaccion = 'Validada'",
            new { IdQR = (int)qr.IdQR });

        if (existing != null)
            return new ValidatePaymentResponse
            {
                IdTransaccion = existing.IdTransaccion, Estado = "Validada",
                Monto = (decimal)qr.Monto, Mensaje = "Transacción ya procesada (idempotente)"
            };

        // Call BCP Mock
        decimal monto = (decimal)qr.Monto;
        var payload = new { codigoQr = req.CodigoQr, monto };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        HttpResponseMessage? bcpResp = null;
        try
        {
            bcpResp = await _httpClient.PostAsync("/api/bcp/validate", content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling BcpMockService");
            return new ValidatePaymentResponse { Estado = "Error", Mensaje = "Error al contactar servicio BCP" };
        }

        var bcpBody = await bcpResp.Content.ReadAsStringAsync();
        var bcpResult = JsonSerializer.Deserialize<JsonElement>(bcpBody);
        var estado = bcpResult.GetProperty("estado").GetString() ?? "Rechazado";
        var referencia = bcpResult.TryGetProperty("referencia", out var refEl) && refEl.ValueKind != JsonValueKind.Null
            ? refEl.GetString() : null;

        var estadoTx = estado == "Confirmado" ? "Validada" : "Rechazada";
        var idTx = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Transacciones (EstadoTransaccion, IdQR) VALUES (@Estado, @IdQR);
              SELECT SCOPE_IDENTITY();",
            new { Estado = estadoTx, IdQR = (int)qr.IdQR });

        if (estadoTx == "Validada")
        {
            await conn.ExecuteAsync(
                "INSERT INTO Pagos (MontoPago, IdTransaccion) VALUES (@Monto, @IdTx)",
                new { Monto = monto, IdTx = idTx });
        }

        return new ValidatePaymentResponse
        {
            IdTransaccion = idTx, Estado = estadoTx,
            Referencia = referencia, Monto = monto,
            Mensaje = estadoTx == "Validada" ? "Pago validado correctamente" : "Pago rechazado por BCP"
        };
    }

    public async Task<PagedResult<Transaccion>> GetAllAsync(TransactionFilter filter)
    {
        using var conn = new SqlConnection(_connStr);
        var where = new List<string>();
        var p = new DynamicParameters();

        if (filter.StartDate.HasValue) { where.Add("t.FechaTransaccion >= @StartDate"); p.Add("StartDate", filter.StartDate); }
        if (filter.EndDate.HasValue) { where.Add("t.FechaTransaccion <= @EndDate"); p.Add("EndDate", filter.EndDate); }
        if (!string.IsNullOrEmpty(filter.Estado)) { where.Add("t.EstadoTransaccion = @Estado"); p.Add("Estado", filter.Estado); }
        if (filter.MinMonto.HasValue) { where.Add("q.Monto >= @MinMonto"); p.Add("MinMonto", filter.MinMonto); }
        if (filter.MaxMonto.HasValue) { where.Add("q.Monto <= @MaxMonto"); p.Add("MaxMonto", filter.MaxMonto); }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var countSql = $"SELECT COUNT(*) FROM Transacciones t LEFT JOIN CodigosQR q ON t.IdQR = q.IdQR {whereClause}";
        var total = await conn.ExecuteScalarAsync<int>(countSql, p);

        var offset = (filter.Page - 1) * filter.PageSize;
        p.Add("Offset", offset); p.Add("PageSize", filter.PageSize);
        var sql = $@"SELECT t.IdTransaccion, t.FechaTransaccion, t.EstadoTransaccion, t.IdQR,
                     q.CodigoQR, q.Monto, e.NombreEmpresa
                     FROM Transacciones t
                     LEFT JOIN CodigosQR q ON t.IdQR = q.IdQR
                     LEFT JOIN Empresas e ON q.IdEmpresa = e.IdEmpresa
                     {whereClause} ORDER BY t.FechaTransaccion DESC
                     OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await conn.QueryAsync<Transaccion>(sql, p);
        return new PagedResult<Transaccion> { Data = data, Total = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<TransaccionDetalle?> GetByIdAsync(int id)
    {
        using var conn = new SqlConnection(_connStr);
        var tx = await conn.QueryFirstOrDefaultAsync<TransaccionDetalle>(
            @"SELECT t.IdTransaccion, t.FechaTransaccion, t.EstadoTransaccion, t.IdQR,
                     q.CodigoQR, q.Monto, e.NombreEmpresa
              FROM Transacciones t
              LEFT JOIN CodigosQR q ON t.IdQR = q.IdQR
              LEFT JOIN Empresas e ON q.IdEmpresa = e.IdEmpresa
              WHERE t.IdTransaccion = @Id", new { Id = id });

        if (tx == null) return null;

        var pago = await conn.QueryFirstOrDefaultAsync<Pago>(
            "SELECT * FROM Pagos WHERE IdTransaccion = @Id", new { Id = id });
        tx.Pago = pago;
        return tx;
    }
}

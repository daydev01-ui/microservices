using ClosedXML.Excel;
using Dapper;
using Microsoft.Data.SqlClient;
using ReportingService.Models;

namespace ReportingService.Services;

public class ReportService
{
    private readonly string _connStr;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IConfiguration config, ILogger<ReportService> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task<ReportSummary> GetSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        using var conn = new SqlConnection(_connStr);
        var where = new List<string>();
        var p = new DynamicParameters();
        if (startDate.HasValue) { where.Add("t.FechaTransaccion >= @StartDate"); p.Add("StartDate", startDate); }
        if (endDate.HasValue) { where.Add("t.FechaTransaccion <= @EndDate"); p.Add("EndDate", endDate); }
        var wc = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        var txList = await conn.QueryAsync<TransaccionReporte>($@"
            SELECT t.IdTransaccion, t.FechaTransaccion, t.EstadoTransaccion,
                   q.CodigoQR, q.Monto, e.NombreEmpresa, pg.MontoPago
            FROM Transacciones t
            LEFT JOIN CodigosQR q ON t.IdQR = q.IdQR
            LEFT JOIN Empresas e ON q.IdEmpresa = e.IdEmpresa
            LEFT JOIN Pagos pg ON t.IdTransaccion = pg.IdTransaccion
            {wc} ORDER BY t.FechaTransaccion DESC", p);

        var list = txList.ToList();
        var validated = list.Count(x => x.EstadoTransaccion == "Validada");
        var total = list.Count;
        var monto = list.Where(x => x.MontoPago > 0).Sum(x => x.MontoPago ?? 0);

        return new ReportSummary
        {
            TotalTransacciones = total,
            TotalValidadas = validated,
            TotalRechazadas = total - validated,
            MontoTotal = monto,
            TasaExito = total > 0 ? Math.Round((decimal)validated / total * 100, 2) : 0,
            Transacciones = list
        };
    }

    public async Task<byte[]> ExportExcelAsync(DateTime? startDate, DateTime? endDate)
    {
        var summary = await GetSummaryAsync(startDate, endDate);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reporte");

        ws.Cell(1, 1).Value = "Reporte de Transacciones BCP - QR Payment System";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Resumen";
        ws.Cell(3, 1).Style.Font.Bold = true;
        ws.Cell(4, 1).Value = "Total Transacciones:"; ws.Cell(4, 2).Value = summary.TotalTransacciones;
        ws.Cell(5, 1).Value = "Validadas:"; ws.Cell(5, 2).Value = summary.TotalValidadas;
        ws.Cell(6, 1).Value = "Rechazadas:"; ws.Cell(6, 2).Value = summary.TotalRechazadas;
        ws.Cell(7, 1).Value = "Monto Total (BOB):"; ws.Cell(7, 2).Value = summary.MontoTotal;
        ws.Cell(8, 1).Value = "Tasa de Éxito (%):"; ws.Cell(8, 2).Value = summary.TasaExito;

        int headerRow = 10;
        var headers = new[] { "ID", "Fecha", "Estado", "Código QR", "Empresa", "Monto (BOB)", "Monto Pago (BOB)" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(headerRow, i + 1).Value = headers[i];
            ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
            ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        int row = headerRow + 1;
        foreach (var tx in summary.Transacciones)
        {
            ws.Cell(row, 1).Value = tx.IdTransaccion;
            ws.Cell(row, 2).Value = tx.FechaTransaccion.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 3).Value = tx.EstadoTransaccion;
            ws.Cell(row, 4).Value = tx.CodigoQR;
            ws.Cell(row, 5).Value = tx.NombreEmpresa ?? "";
            ws.Cell(row, 6).Value = tx.Monto ?? 0;
            ws.Cell(row, 7).Value = tx.MontoPago ?? 0;
            row++;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<RealTimeStats> GetRealTimeStatsAsync()
    {
        using var conn = new SqlConnection(_connStr);
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var stats = await conn.QueryFirstOrDefaultAsync<RealTimeStats>(@"
            SELECT 
                COUNT(*) AS TransaccionesHoy,
                SUM(CASE WHEN EstadoTransaccion='Validada' THEN 1 ELSE 0 END) AS ValidadasHoy,
                SUM(CASE WHEN EstadoTransaccion='Rechazada' THEN 1 ELSE 0 END) AS RechazadasHoy,
                ISNULL((SELECT SUM(p.MontoPago) FROM Pagos p 
                        INNER JOIN Transacciones t2 ON p.IdTransaccion = t2.IdTransaccion
                        WHERE t2.FechaTransaccion >= @Today AND t2.FechaTransaccion < @Tomorrow), 0) AS MontoHoy
            FROM Transacciones WHERE FechaTransaccion >= @Today AND FechaTransaccion < @Tomorrow",
            new { Today = today, Tomorrow = tomorrow });
        return stats ?? new RealTimeStats();
    }
}

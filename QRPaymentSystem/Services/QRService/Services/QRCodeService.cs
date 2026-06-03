using Dapper;
using Microsoft.Data.SqlClient;
using QRService.Models;
using QRCoder;

namespace QRService.Services;

public class QRCodeService
{
    private readonly string _connStr;
    private readonly ILogger<QRCodeService> _logger;

    public QRCodeService(IConfiguration config, ILogger<QRCodeService> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task<GenerateQRResponse> GenerateAsync(GenerateQRRequest req)
    {
        var codigo = $"QR-BCP-{Guid.NewGuid():N}".ToUpper();
        using var conn = new SqlConnection(_connStr);
        var id = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO CodigosQR (CodigoQR, Monto, IdEmpresa) VALUES (@CodigoQR, @Monto, @IdEmpresa);
              SELECT SCOPE_IDENTITY();",
            new { CodigoQR = codigo, req.Monto, IdEmpresa = req.EmpresaId });

        var base64 = GenerateQRImage(codigo);
        _logger.LogInformation("QR generated: {Codigo}", codigo);

        return new GenerateQRResponse
        {
            IdQR = id, CodigoQR = codigo, Monto = req.Monto,
            QRImageBase64 = base64, FechaCreacion = DateTime.UtcNow, EstadoQR = "Activo"
        };
    }

    private string GenerateQRImage(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(pngBytes);
    }

    public async Task<IEnumerable<CodigoQR>> GetAllAsync()
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryAsync<CodigoQR>(
            @"SELECT q.IdQR, q.CodigoQR AS CodigoQR_, q.Monto, q.FechaCreacion, q.EstadoQR, q.IdEmpresa, e.NombreEmpresa
              FROM CodigosQR q LEFT JOIN Empresas e ON q.IdEmpresa = e.IdEmpresa
              ORDER BY q.FechaCreacion DESC");
    }

    public async Task<CodigoQR?> GetByIdAsync(int id)
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryFirstOrDefaultAsync<CodigoQR>(
            @"SELECT q.IdQR, q.CodigoQR AS CodigoQR_, q.Monto, q.FechaCreacion, q.EstadoQR, q.IdEmpresa, e.NombreEmpresa
              FROM CodigosQR q LEFT JOIN Empresas e ON q.IdEmpresa = e.IdEmpresa
              WHERE q.IdQR = @Id",
            new { Id = id });
    }

    public async Task<bool> UpdateStatusAsync(int id, string estado)
    {
        using var conn = new SqlConnection(_connStr);
        var rows = await conn.ExecuteAsync(
            "UPDATE CodigosQR SET EstadoQR=@Estado WHERE IdQR=@Id",
            new { Estado = estado, Id = id });
        return rows > 0;
    }
}

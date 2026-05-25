namespace BCP.Payments.API.Infrastructure.ExternalServices;

using BCP.Payments.API.Domain.Interfaces;
using System.Text;
using System.Text.Json;

public class BCPHttpClient : IBCPExternalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BCPHttpClient> _logger;

    public BCPHttpClient(HttpClient httpClient, ILogger<BCPHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RespuestaBCP> VerificarPagoAsync(
        string referenciaCliente, decimal monto, Guid sucursalId, CancellationToken ct = default)
    {
        var body = new { referenciaCliente, monto, sucursalId = sucursalId.ToString() };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        _logger.LogInformation("Llamando BCP Stub: referencia={Ref} monto={Monto}", referenciaCliente, monto);

        var response = await _httpClient.PostAsync("api/bcp/verificar-pago", content, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<BCPStubResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Respuesta inválida del BCP");

        return new RespuestaBCP(
            result.ReferenciaExterna ?? Guid.NewGuid().ToString(),
            result.Estado,
            result.CodigoAutorizacion,
            result.Mensaje ?? "Sin mensaje");
    }

    private record BCPStubResponse(
        string? ReferenciaExterna, string Estado,
        string? CodigoAutorizacion, string? Mensaje);
}

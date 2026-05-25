namespace BCP.Payments.API.Domain.Interfaces;

using BCP.Payments.API.Domain.Entities;

public interface ITransaccionRepository
{
    Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Transaccion?> ObtenerPorReferenciaClienteAsync(string referencia, CancellationToken ct = default);
    Task<List<Transaccion>> ListarPorSucursalAsync(Guid sucursalId, DateTime? desde, DateTime? hasta, CancellationToken ct = default);
    Task<List<Transaccion>> ListarPorUsuarioAsync(Guid usuarioId, DateTime? desde, DateTime? hasta, CancellationToken ct = default);
    Task GuardarAsync(Transaccion transaccion, CancellationToken ct = default);
    Task ActualizarAsync(Transaccion transaccion, CancellationToken ct = default);
    Task<decimal> SumarMontoSucursalHoyAsync(Guid sucursalId, CancellationToken ct = default);
    Task<int> ContarTransaccionesSucursalHoyAsync(Guid sucursalId, CancellationToken ct = default);
}

public interface IBCPExternalService
{
    Task<RespuestaBCP> VerificarPagoAsync(string referenciaCliente, decimal monto, Guid sucursalId, CancellationToken ct = default);
}

public record RespuestaBCP(
    string ReferenciaExterna, string Estado,
    string? CodigoAutorizacion, string Mensaje);

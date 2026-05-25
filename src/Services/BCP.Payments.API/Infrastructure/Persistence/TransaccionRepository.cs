namespace BCP.Payments.API.Infrastructure.Persistence;

using BCP.Payments.API.Domain.Entities;
using BCP.Payments.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class TransaccionRepository : ITransaccionRepository
{
    private readonly PaymentsDbContext _ctx;
    public TransaccionRepository(PaymentsDbContext ctx) => _ctx = ctx;

    public Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => _ctx.Transacciones.FindAsync([id], ct).AsTask()!;

    public Task<Transaccion?> ObtenerPorReferenciaClienteAsync(string ref_, CancellationToken ct = default)
        => _ctx.Transacciones.FirstOrDefaultAsync(t => t.ReferenciaCliente == ref_, ct);

    public Task<List<Transaccion>> ListarPorSucursalAsync(Guid sucursalId, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
        => _ctx.Transacciones
            .Where(t => t.IdSucursal == sucursalId
                && (desde == null || t.FechaHora >= desde)
                && (hasta == null || t.FechaHora <= hasta))
            .OrderByDescending(t => t.FechaHora)
            .ToListAsync(ct);

    public Task<List<Transaccion>> ListarPorUsuarioAsync(Guid usuarioId, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
        => _ctx.Transacciones
            .Where(t => t.IdUsuario == usuarioId
                && (desde == null || t.FechaHora >= desde)
                && (hasta == null || t.FechaHora <= hasta))
            .OrderByDescending(t => t.FechaHora)
            .ToListAsync(ct);

    public async Task GuardarAsync(Transaccion transaccion, CancellationToken ct = default)
    {
        _ctx.Transacciones.Add(transaccion);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Transaccion transaccion, CancellationToken ct = default)
    {
        _ctx.Transacciones.Update(transaccion);
        await _ctx.SaveChangesAsync(ct);
    }

    public Task<decimal> SumarMontoSucursalHoyAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var hoy = DateTime.UtcNow.Date;
        return _ctx.Transacciones
            .Where(t => t.IdSucursal == sucursalId
                && t.FechaHora >= hoy
                && t.Estado == Domain.Enums.EstadoTransaccion.Confirmada)
            .SumAsync(t => (decimal?)t.Monto, ct)
            .ContinueWith(t => t.Result ?? 0m, ct);
    }

    public Task<int> ContarTransaccionesSucursalHoyAsync(Guid sucursalId, CancellationToken ct = default)
    {
        var hoy = DateTime.UtcNow.Date;
        return _ctx.Transacciones
            .CountAsync(t => t.IdSucursal == sucursalId && t.FechaHora >= hoy, ct);
    }
}

namespace BCP.Auth.API.Domain.Interfaces;

using BCP.Auth.API.Domain.Entities;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task GuardarAsync(Usuario usuario, CancellationToken ct = default);
    Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
}

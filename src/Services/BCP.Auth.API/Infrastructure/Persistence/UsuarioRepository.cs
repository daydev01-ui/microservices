namespace BCP.Auth.API.Infrastructure.Persistence;

using BCP.Auth.API.Domain.Entities;
using BCP.Auth.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AuthDbContext _context;

    public UsuarioRepository(AuthDbContext context) => _context = context;

    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default)
        => await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Usuarios.FindAsync([id], ct);

    public async Task GuardarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ActualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(ct);
    }
}

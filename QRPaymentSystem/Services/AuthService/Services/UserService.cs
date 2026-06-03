using Dapper;
using Microsoft.Data.SqlClient;
using AuthService.Models;
using BCrypt.Net;

namespace AuthService.Services;

public class UserService
{
    private readonly string _connStr;
    private readonly ILogger<UserService> _logger;

    public UserService(IConfiguration config, ILogger<UserService> logger)
    {
        _connStr = config.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryAsync<Usuario>(
            @"SELECT u.IdUsuario, u.NombreCompleto, u.CorreoElectronico, u.EstadoUsuario, 
                     u.IdRol, u.IdEmpresa, r.NombreRol, e.NombreEmpresa
              FROM Usuarios u
              LEFT JOIN Roles r ON u.IdRol = r.IdRol
              LEFT JOIN Empresas e ON u.IdEmpresa = e.IdEmpresa");
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryFirstOrDefaultAsync<Usuario>(
            @"SELECT u.IdUsuario, u.NombreCompleto, u.CorreoElectronico, u.EstadoUsuario,
                     u.IdRol, u.IdEmpresa, r.NombreRol, e.NombreEmpresa
              FROM Usuarios u
              LEFT JOIN Roles r ON u.IdRol = r.IdRol
              LEFT JOIN Empresas e ON u.IdEmpresa = e.IdEmpresa
              WHERE u.IdUsuario = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(CreateUserRequest req)
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(req.Contraseña);
        using var conn = new SqlConnection(_connStr);
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Usuarios (NombreCompleto, CorreoElectronico, Contraseña, IdRol, IdEmpresa)
              VALUES (@NombreCompleto, @CorreoElectronico, @Contraseña, @IdRol, @IdEmpresa);
              SELECT SCOPE_IDENTITY();",
            new { req.NombreCompleto, req.CorreoElectronico, Contraseña = hashed, req.IdRol, req.IdEmpresa });
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest req)
    {
        using var conn = new SqlConnection(_connStr);
        var rows = await conn.ExecuteAsync(
            @"UPDATE Usuarios SET NombreCompleto=@NombreCompleto, CorreoElectronico=@CorreoElectronico,
              EstadoUsuario=@EstadoUsuario, IdRol=@IdRol, IdEmpresa=@IdEmpresa WHERE IdUsuario=@Id",
            new { req.NombreCompleto, req.CorreoElectronico, req.EstadoUsuario, req.IdRol, req.IdEmpresa, Id = id });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = new SqlConnection(_connStr);
        var rows = await conn.ExecuteAsync(
            "UPDATE Usuarios SET EstadoUsuario='Inactivo' WHERE IdUsuario=@Id", new { Id = id });
        return rows > 0;
    }

    public async Task<bool> AssignRoleAsync(int userId, int roleId)
    {
        using var conn = new SqlConnection(_connStr);
        var rows = await conn.ExecuteAsync(
            "UPDATE Usuarios SET IdRol=@IdRol WHERE IdUsuario=@IdUsuario",
            new { IdRol = roleId, IdUsuario = userId });
        return rows > 0;
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryAsync<Role>("SELECT * FROM Roles");
    }

    public async Task<IEnumerable<Empresa>> GetEmpresasAsync()
    {
        using var conn = new SqlConnection(_connStr);
        return await conn.QueryAsync<Empresa>("SELECT * FROM Empresas WHERE EstadoEmpresa='Activo'");
    }
}

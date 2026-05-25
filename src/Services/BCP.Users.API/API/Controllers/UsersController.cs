namespace BCP.Users.API.API.Controllers;

using BCP.Shared;
using BCP.Users.API.Application.DTOs;
using BCP.Users.API.Domain.Entities;
using BCP.Users.API.Domain.Events;
using BCP.Users.API.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UsersDbContext _ctx;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UsersDbContext ctx, IPublishEndpoint publisher, ILogger<UsersController> logger)
    {
        _ctx = ctx;
        _publisher = publisher;
        _logger = logger;
    }

    // ═══ EMPRESAS ═══

    [HttpGet("empresas")]
    public async Task<ActionResult<ApiResponse<List<EmpresaDto>>>> ListarEmpresas(CancellationToken ct)
    {
        var empresas = await _ctx.Empresas.ToListAsync(ct);
        var dtos = new List<EmpresaDto>();
        foreach (var e in empresas)
        {
            var sucCount = await _ctx.Sucursales.CountAsync(s => s.IdEmpresa == e.IdEmpresa, ct);
            dtos.Add(new EmpresaDto
            {
                IdEmpresa = e.IdEmpresa, Nombre = e.Nombre, NIT = e.NIT,
                RazonSocial = e.RazonSocial, Estado = e.Estado,
                FechaRegistro = e.FechaRegistro, TotalSucursales = sucCount
            });
        }
        return Ok(ApiResponse<List<EmpresaDto>>.Ok(dtos));
    }

    [HttpPost("empresas")]
    public async Task<ActionResult<ApiResponse<EmpresaDto>>> CrearEmpresa(
        [FromBody] CrearEmpresaRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<EmpresaDto>.Fail("Datos inválidos"));

        var existe = await _ctx.Empresas.AnyAsync(e => e.NIT == request.NIT, ct);
        if (existe) return Conflict(ApiResponse<EmpresaDto>.Fail($"Ya existe una empresa con NIT {request.NIT}"));

        var empresa = Empresa.Crear(request.Nombre, request.NIT, request.RazonSocial);
        _ctx.Empresas.Add(empresa);
        await _ctx.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(ObtenerEmpresa), new { id = empresa.IdEmpresa },
            ApiResponse<EmpresaDto>.Ok(MapearEmpresa(empresa, 0), "Empresa creada exitosamente"));
    }

    [HttpGet("empresas/{id}")]
    public async Task<ActionResult<ApiResponse<EmpresaDto>>> ObtenerEmpresa(Guid id, CancellationToken ct)
    {
        var empresa = await _ctx.Empresas.FindAsync([id], ct);
        if (empresa is null) return NotFound(ApiResponse<EmpresaDto>.Fail("Empresa no encontrada"));

        var sucCount = await _ctx.Sucursales.CountAsync(s => s.IdEmpresa == id, ct);
        return Ok(ApiResponse<EmpresaDto>.Ok(MapearEmpresa(empresa, sucCount)));
    }

    [HttpPut("empresas/{id}")]
    public async Task<ActionResult<ApiResponse<EmpresaDto>>> ActualizarEmpresa(
        Guid id, [FromBody] ActualizarEmpresaRequest request, CancellationToken ct)
    {
        var empresa = await _ctx.Empresas.FindAsync([id], ct);
        if (empresa is null) return NotFound(ApiResponse<EmpresaDto>.Fail("Empresa no encontrada"));

        empresa.Actualizar(request.Nombre, request.RazonSocial);
        await _ctx.SaveChangesAsync(ct);
        var sucCount = await _ctx.Sucursales.CountAsync(s => s.IdEmpresa == id, ct);
        return Ok(ApiResponse<EmpresaDto>.Ok(MapearEmpresa(empresa, sucCount), "Empresa actualizada"));
    }

    // ═══ SUCURSALES ═══

    [HttpGet("empresas/{id}/sucursales")]
    public async Task<ActionResult<ApiResponse<List<SucursalDto>>>> ListarSucursales(Guid id, CancellationToken ct)
    {
        var sucursales = await _ctx.Sucursales.Where(s => s.IdEmpresa == id).ToListAsync(ct);
        var dtos = sucursales.Select(s => new SucursalDto
        {
            IdSucursal = s.IdSucursal, IdEmpresa = s.IdEmpresa,
            Nombre = s.Nombre, Direccion = s.Direccion, Estado = s.Estado
        }).ToList();
        return Ok(ApiResponse<List<SucursalDto>>.Ok(dtos));
    }

    [HttpPost("sucursales")]
    public async Task<ActionResult<ApiResponse<SucursalDto>>> CrearSucursal(
        [FromBody] CrearSucursalRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<SucursalDto>.Fail("Datos inválidos"));

        var empresa = await _ctx.Empresas.FindAsync([request.IdEmpresa], ct);
        if (empresa is null) return NotFound(ApiResponse<SucursalDto>.Fail("Empresa no encontrada"));

        var sucursal = Sucursal.Crear(request.IdEmpresa, request.Nombre, request.Direccion);
        _ctx.Sucursales.Add(sucursal);
        await _ctx.SaveChangesAsync(ct);

        return Created("", ApiResponse<SucursalDto>.Ok(new SucursalDto
        {
            IdSucursal = sucursal.IdSucursal, IdEmpresa = sucursal.IdEmpresa,
            Nombre = sucursal.Nombre, Direccion = sucursal.Direccion, Estado = sucursal.Estado
        }, "Sucursal creada exitosamente"));
    }

    [HttpPut("sucursales/{id}")]
    public async Task<ActionResult<ApiResponse<SucursalDto>>> ActualizarSucursal(
        Guid id, [FromBody] ActualizarSucursalRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<SucursalDto>.Fail("Datos inválidos"));

        var sucursal = await _ctx.Sucursales.FindAsync([id], ct);
        if (sucursal is null) return NotFound(ApiResponse<SucursalDto>.Fail("Sucursal no encontrada"));

        sucursal.Actualizar(request.Nombre, request.Direccion);
        await _ctx.SaveChangesAsync(ct);

        return Ok(ApiResponse<SucursalDto>.Ok(new SucursalDto
        {
            IdSucursal = sucursal.IdSucursal, IdEmpresa = sucursal.IdEmpresa,
            Nombre = sucursal.Nombre, Direccion = sucursal.Direccion, Estado = sucursal.Estado
        }, "Sucursal actualizada exitosamente"));
    }

    // ═══ USUARIOS ═══

    [HttpGet("usuarios")]
    public async Task<ActionResult<ApiResponse<List<UsuarioDto>>>> ListarUsuarios(
        [FromQuery] Guid? idEmpresa, CancellationToken ct)
    {
        var query = _ctx.Usuarios.AsQueryable();
        if (idEmpresa.HasValue) query = query.Where(u => u.IdEmpresa == idEmpresa);

        var usuarios = await query.OrderBy(u => u.Nombre).ToListAsync(ct);
        var dtos = usuarios.Select(u => new UsuarioDto
        {
            IdUsuario = u.IdUsuario, Nombre = u.Nombre, Email = u.Email,
            Rol = u.Rol, Estado = u.Estado, IdEmpresa = u.IdEmpresa,
            IdSucursal = u.IdSucursal, FechaCreacion = u.FechaCreacion
        }).ToList();

        return Ok(ApiResponse<List<UsuarioDto>>.Ok(dtos));
    }

    [HttpPost("usuarios")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> CrearUsuario(
        [FromBody] CrearUsuarioRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<UsuarioDto>.Fail("Datos inválidos"));

        var existe = await _ctx.Usuarios.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);
        if (existe) return Conflict(ApiResponse<UsuarioDto>.Fail("El email ya está registrado"));

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var usuario = Usuario.Crear(request.Nombre, request.Email, hash, request.Rol,
            request.IdEmpresa, request.IdSucursal);

        _ctx.Usuarios.Add(usuario);
        await _ctx.SaveChangesAsync(ct);

        // Publicar evento de usuario creado
        try
        {
            await _publisher.Publish(new UsuarioCreadoEvent(
                usuario.IdUsuario, usuario.Nombre, usuario.Email, usuario.Rol,
                usuario.IdEmpresa, usuario.IdSucursal), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo publicar evento UsuarioCreado");
        }

        var dto = new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario, Nombre = usuario.Nombre, Email = usuario.Email,
            Rol = usuario.Rol, Estado = usuario.Estado, IdEmpresa = usuario.IdEmpresa,
            IdSucursal = usuario.IdSucursal, FechaCreacion = usuario.FechaCreacion
        };
        return Created("", ApiResponse<UsuarioDto>.Ok(dto, "Usuario creado exitosamente"));
    }

    [HttpGet("usuarios/{id}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObtenerUsuario(Guid id, CancellationToken ct)
    {
        var u = await _ctx.Usuarios.FindAsync([id], ct);
        if (u is null) return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));

        return Ok(ApiResponse<UsuarioDto>.Ok(new UsuarioDto
        {
            IdUsuario = u.IdUsuario, Nombre = u.Nombre, Email = u.Email,
            Rol = u.Rol, Estado = u.Estado, IdEmpresa = u.IdEmpresa,
            IdSucursal = u.IdSucursal, FechaCreacion = u.FechaCreacion
        }));
    }

    [HttpPut("usuarios/{id}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ActualizarUsuario(
        Guid id, [FromBody] ActualizarUsuarioRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<UsuarioDto>.Fail("Datos inválidos"));

        var usuario = await _ctx.Usuarios.FindAsync([id], ct);
        if (usuario is null) return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));

        usuario.Actualizar(request.Nombre, request.Rol, request.IdSucursal);
        await _ctx.SaveChangesAsync(ct);

        return Ok(ApiResponse<UsuarioDto>.Ok(new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario, Nombre = usuario.Nombre, Email = usuario.Email,
            Rol = usuario.Rol, Estado = usuario.Estado, IdEmpresa = usuario.IdEmpresa,
            IdSucursal = usuario.IdSucursal, FechaCreacion = usuario.FechaCreacion
        }, "Usuario actualizado exitosamente"));
    }

    [HttpDelete("usuarios/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DesactivarUsuario(Guid id, CancellationToken ct)
    {
        var usuario = await _ctx.Usuarios.FindAsync([id], ct);
        if (usuario is null) return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        usuario.Desactivar();
        await _ctx.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("Usuario desactivado"));
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
        => Ok(new { status = "healthy", service = "BCP.Users.API", timestamp = DateTime.UtcNow });

    private static EmpresaDto MapearEmpresa(Empresa e, int sucCount) => new()
    {
        IdEmpresa = e.IdEmpresa, Nombre = e.Nombre, NIT = e.NIT,
        RazonSocial = e.RazonSocial, Estado = e.Estado,
        FechaRegistro = e.FechaRegistro, TotalSucursales = sucCount
    };
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        var id = await _userService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id }, new { IdUsuario = id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var ok = await _userService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _userService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles() => Ok(await _userService.GetRolesAsync());

    [HttpGet("empresas")]
    public async Task<IActionResult> GetEmpresas() => Ok(await _userService.GetEmpresasAsync());
}

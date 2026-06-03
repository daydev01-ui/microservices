using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly UserService _userService;

    public RolesController(UserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _userService.GetRolesAsync());

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRoleRequest req)
    {
        var ok = await _userService.AssignRoleAsync(req.IdUsuario, req.IdRol);
        return ok ? Ok(new { mensaje = "Rol asignado correctamente" }) : NotFound();
    }
}

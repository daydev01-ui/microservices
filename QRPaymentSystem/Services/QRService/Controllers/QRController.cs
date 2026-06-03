using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRService.Models;
using QRService.Services;

namespace QRService.Controllers;

[ApiController]
[Route("api/qr")]
[Authorize]
public class QRController : ControllerBase
{
    private readonly QRCodeService _qrService;

    public QRController(QRCodeService qrService) => _qrService = qrService;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateQRRequest req)
    {
        if (req.Monto <= 0) return BadRequest(new { mensaje = "Monto debe ser mayor a 0" });
        var result = await _qrService.GenerateAsync(req);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _qrService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var qr = await _qrService.GetByIdAsync(id);
        return qr == null ? NotFound() : Ok(qr);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
    {
        var ok = await _qrService.UpdateStatusAsync(id, req.Estado);
        return ok ? NoContent() : NotFound();
    }
}

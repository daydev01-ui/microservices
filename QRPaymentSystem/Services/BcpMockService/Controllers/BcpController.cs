using Microsoft.AspNetCore.Mvc;

namespace BcpMockService.Controllers;

public class ValidateRequest
{
    public string CodigoQr { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

[ApiController]
[Route("api/bcp")]
public class BcpController : ControllerBase
{
    private readonly ILogger<BcpController> _logger;

    public BcpController(ILogger<BcpController> logger) => _logger = logger;

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateRequest req)
    {
        _logger.LogInformation("BCP validate: QR={QR} Monto={Monto}", req.CodigoQr, req.Monto);
        var delay = Random.Shared.Next(300, 801);
        await Task.Delay(delay);

        if (req.Monto > 0)
        {
            var ref_ = "BCP-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            return Ok(new { estado = "Confirmado", referencia = ref_ });
        }
        return Ok(new { estado = "Rechazado", referencia = (string?)null });
    }
}

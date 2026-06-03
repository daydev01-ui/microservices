using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Models;
using TransactionService.Services;

namespace TransactionService.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionServiceImpl _txService;

    public TransactionsController(TransactionServiceImpl txService) => _txService = txService;

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidatePaymentRequest req)
    {
        var result = await _txService.ValidatePaymentAsync(req);
        if (result.Estado == "Error") return BadRequest(result);
        if (result.Mensaje.Contains("idempotente")) return Conflict(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TransactionFilter filter)
        => Ok(await _txService.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tx = await _txService.GetByIdAsync(id);
        return tx == null ? NotFound() : Ok(tx);
    }
}

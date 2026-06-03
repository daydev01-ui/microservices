using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ReportingService.Hubs;
using ReportingService.Services;

namespace ReportingService.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly IHubContext<ReportsHub> _hub;

    public ReportsController(ReportService reportService, IHubContext<ReportsHub> hub)
    {
        _reportService = reportService;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        => Ok(await _reportService.GetSummaryAsync(startDate, endDate));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format = "excel",
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (format.ToLower() != "excel")
            return BadRequest(new { mensaje = "Solo se soporta formato excel" });
        var bytes = await _reportService.ExportExcelAsync(startDate, endDate);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reporte_transacciones_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("realtime")]
    public async Task<IActionResult> RealTime()
    {
        var stats = await _reportService.GetRealTimeStatsAsync();
        await _hub.Clients.Group("reports").SendAsync("statsUpdated", stats);
        return Ok(stats);
    }
}

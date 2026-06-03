using Microsoft.AspNetCore.SignalR;

namespace ReportingService.Hubs;

public class ReportsHub : Hub
{
    public async Task JoinReports()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "reports");
    }
}

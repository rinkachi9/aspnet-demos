using Microsoft.AspNetCore.SignalR;

namespace AdvancedRealTime.Hubs;

public class NotificationHub : Hub
{
    // Called when a client connects
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GlobalAlerts");
        await base.OnConnectedAsync();
    }

    // Client can call this method to broadcast to everyone
    public async Task BroadcastMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    
    // Server-side code usually calls IHubContext<NotificationHub> to push messages, 
    // rather than the client invoking methods directly.
}

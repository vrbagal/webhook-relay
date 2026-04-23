using Microsoft.AspNetCore.SignalR;

namespace WebhookRelay.Api.Hubs;

public class WebhookRelayHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await base.OnConnectedAsync();
    }
}

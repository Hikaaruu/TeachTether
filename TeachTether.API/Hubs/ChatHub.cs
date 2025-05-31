using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TeachTether.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinThread(int threadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread-{threadId}");
    }

    public async Task LeaveThread(int threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread-{threadId}");
    }
}
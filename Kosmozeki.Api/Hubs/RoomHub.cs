using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Kosmozeki.Api.Hubs;

public sealed class RoomHub : Hub
{
    public Task JoinRoom(string roomId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");

    public Task LeaveRoom(string roomId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomId}");
}

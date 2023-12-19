using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

// Display the presence of a user in the application
[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _tracker;

    public PresenceHub(PresenceTracker tracker)
    {
        _tracker = tracker;
    }
    
    public override async Task OnConnectedAsync()
    {
        var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId); // Adds the tracker specified in ApplicationServiceExtensions.cs
        if (isOnline)
        {
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername()); // Users connected to this hub will receive a notification of the 'username' that has just connected
        }

        var currentUsers = await _tracker.GetOnlineUsers(); // Gets a list of the current users
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers); // Sends a list to the calling client when somebody connects
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

        if(isOffline)
        {
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername()); // Idicates user being offline
        }

        await base.OnDisconnectedAsync(exception);
    }
}

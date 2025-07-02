using DatingApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.SignalR
{
    [Authorize]
    public class PresenceHub:Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            if (Context.User == null) throw new HubException("User not found");
          var isOnline=  await _tracker.UserConnected(Context.User.GetUsername(),Context.ConnectionId);
          if (isOnline)  await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());
            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
          
        }

        public  override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User == null) throw new HubException("User not found");
           var isOffline =  await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
          if (isOffline)  await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());
            //var currentUsers = await _tracker.GetOnlineUsers();
            //await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            await base.OnConnectedAsync();
        }
    }
}

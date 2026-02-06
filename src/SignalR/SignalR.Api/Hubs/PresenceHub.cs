using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace SignalR.Api.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IConnectionMultiplexer _redis;
    private const string ONLINE_USERS_KEY = "presence:online_users";

    public PresenceHub(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public override async Task OnConnectedAsync()
    {
        var db = _redis.GetDatabase();
        var userId = Context.UserIdentifier;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await db.SetAddAsync(ONLINE_USERS_KEY, userId);
            await Clients.Others.SendAsync("UserIsOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var db = _redis.GetDatabase();
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await db.SetRemoveAsync(ONLINE_USERS_KEY, userId);
            await Clients.Others.SendAsync("UserIsOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string[]> GetOnlineUsers()
    {
        var db = _redis.GetDatabase();
        var users = await db.SetMembersAsync(ONLINE_USERS_KEY);
        return users.Select(x => x.ToString()).ToArray();
    }
}

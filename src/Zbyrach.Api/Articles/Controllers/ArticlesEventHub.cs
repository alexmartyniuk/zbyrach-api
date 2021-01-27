using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Articles
{
    [Authorize]
    public class ArticlesEventHub : Hub
    {
        private readonly ILogger<ArticlesEventHub> _logger;
        private readonly ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

        public ArticlesEventHub(ILogger<ArticlesEventHub> logger)
        {
            _logger = logger;
            _connections = new ConcurrentDictionary<string, string>();
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("OnConnectedAsync {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _logger.LogDebug("OnDisconnectedAsync {ConnectionId}, {exception}", Context.ConnectionId, ex?.Message);

            if (_connections.TryGetValue(Context.ConnectionId, out var userId))
            {
                await Unsubscribe(userId);
            }

            await base.OnDisconnectedAsync(ex);
        }

        public async Task Subscribe(string userId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _connections[Context.ConnectionId] = userId;
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong when subscribing user {userId}: {exception}", userId, e.Message);
            }
        }

        public async Task Unsubscribe(string userId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _connections.TryRemove(Context.ConnectionId, out var value);
            }
            catch (Exception e)
            {
                _logger.LogError("Something went wrong when unsubscribing user {userId}: {exception}", userId, e.Message);
            }
        }

        public class User
        {
            public string Id { get; set; }
            public string Connection { get; set; }
        }
    }
}
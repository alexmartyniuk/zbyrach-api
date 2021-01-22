using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Zbyrach.Api.Articles
{
    [Authorize]
    public class ArticlesEventHub: Hub
    {
        private readonly ILogger<ArticlesEventHub> _logger;

        public ArticlesEventHub(ILogger<ArticlesEventHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("OnConnectedAsync {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            _logger.LogDebug("OnDisconnectedAsync {ConnectionId}, {exception}", Context.ConnectionId, ex?.Message);
           
            // implement removing from groups

            await base.OnDisconnectedAsync(ex);
        }

        public async Task Subscribe(string userId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Something went wrong when subscribing user {userId}: {exception}", userId, e.Message);
            }
        }

        public async Task Unsubscribe(string userId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

            }
            catch (Exception e)
            {
                _logger.LogWarning("Something went wrong when unsubscribing user {userId}: {exception}", userId, e.Message);
            }
        }


        public class User
        {
            public string Id { get; set; }
            public string Connection { get; set; }
        }
    }
}
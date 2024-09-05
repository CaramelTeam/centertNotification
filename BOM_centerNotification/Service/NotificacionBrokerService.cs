using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;

namespace BOM_centerNotification.Service
{
    public class NotificacionBrokerService : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

        //public async Task sendMessage(string user, string infoModel)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, infoModel);
        //}

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];

            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, string message)
        {
            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                var usercONECT = connectionId;
                var messageDta = message;
                Clients.All.SendAsync("NotiService", "HolaMundo");
                await Clients.Client(connectionId).SendAsync("NotiService", message);
            }
            return;
        }

        public async Task<List<string>> getConnectionCliente(List<string> userId)
        {
            try
            {
                var listConnectionId = userId.Select(userId =>
                {
                    _userConnections.TryGetValue(userId, out var connectionId);
                    return connectionId ;
                })
                .Where(connet => connet is not null)
                .ToList();
                

                return listConnectionId!;
            }
            catch (Exception err)
            {
                return new List<string>();
            }
        }
    }
}

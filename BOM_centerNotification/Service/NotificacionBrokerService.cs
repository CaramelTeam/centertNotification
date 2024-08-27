using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

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
            //var userId = Context.User.Identity.Name; // Asume que estás utilizando la autenticación
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId; // Guarda la conexión del usuario
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

        public async Task<string> getConnectionCliente(string userId)
        {
            try
            {
                _userConnections.TryGetValue(userId, out var connectionId);
                return connectionId;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
    }
}

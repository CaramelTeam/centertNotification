
using Microsoft.AspNetCore.SignalR;

namespace BOM_centerNotification.Service
{
    public class queueMessageService : BackgroundService
    {
        private readonly IHubContext<NotificacionBrokerService> _serviceNoti;

        private List<DTO_MessageQueue> _MessageQueues = new List<DTO_MessageQueue>();
        public class DTO_MessageQueue
        {
            public string? nameUser { get; set; }
            public List<string>? messages { get; set; }
        }


        public queueMessageService(IHubContext<NotificacionBrokerService> notificationBroker)
        {
            _serviceNoti = notificationBroker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
        }

        public async Task addQueueMessage(string dataRes, string nameUser)
        {
            try
            {
                if (_MessageQueues.Where(d => d.nameUser == nameUser).Any())
                {
                    _MessageQueues.Where(d => d.nameUser == nameUser).FirstOrDefault()!.messages!.Add(dataRes);
                }
                else
                {
                    var newObn = new DTO_MessageQueue()
                    {
                        nameUser = nameUser,
                        messages = new List<string> { dataRes }
                    };
                    _MessageQueues.Add(newObn);
                }

            }
            catch (Exception ex) 
            {
                
            }
        }
    }
}

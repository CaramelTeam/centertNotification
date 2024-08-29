using BOM_centerNotification.Data;
using BOM_centerNotification.Models;
using BOM_centerNotification.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BOM_centerNotification.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        private readonly IHubContext<NotificacionBrokerService> _serviceNoti;

        private readonly AppDBContext _db;

        public WeatherForecastController(IHttpClientFactory httpClient, IHubContext<NotificacionBrokerService> notificationBroker, AppDBContext context)
        {
            _httpClient = httpClient.CreateClient();
            _serviceNoti = notificationBroker;
            _db = context;
        }

        public class DTO_targetInfo
        {
            public string? Status { get; set; }
            public string? Scheme { get; set; }
            public string? Type { get; set; }
            public string? Issuer { get; set; }
            public string? CardTier { get; set; }
            public DTO_Country Country { get; set; }
            public bool Luhn { get; set; }
        }

        public class DTO_Country
        {
            public string? A2 { get; set; }
            public string? A3 { get; set; }
            public string? N3 { get; set; }
            public string? ISD { get; set; }
            public string? Name { get; set; }
            public string? Cont { get; set; }
        }

        public class DTO_sendNotification
        {
            public string? sixDigit { get; set; }
            public List<string>? nameUsers { get; set; }
            public string? userSend { get; set; }
        }

        [HttpPost]
        [Route("set-notificationTargetCredit")]
        public async Task<IActionResult> setNotificationTargetCredit([FromBody] DTO_sendNotification information)
        {
            try
            {
                var endPoint = "https://data.handyapi.com/bin/" + information.sixDigit;
                var response = await _httpClient.GetAsync(endPoint);

                string dataRes = await response.Content.ReadAsStringAsync();
                var dataDes = JsonSerializer.Deserialize<DTO_targetInfo>(dataRes);


                notificationUserFuntion(information.nameUsers!, dataRes);

                saveNotificationDB(information!, dataRes);

                return Ok(dataDes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task saveNotificationDB(DTO_sendNotification dataSave, string dataRes)
        {
            try
            {

             List<DTO_QueueMessage> dataSaveOnDB = dataSave.nameUsers!.Select(user =>
            {
                DTO_QueueMessage country = new DTO_QueueMessage()
                {
                    nameUser = user,
                    message = dataRes,
                    confirmRead = false,
                    registeNotification = DateTime.Now,
                    userSend = dataSave.userSend
                };

                return country;
            }).ToList();


                _db.DTO_QueueMessage.AddRange(dataSaveOnDB);
                _db.SaveChanges();

            }
            catch (Exception err)
            {
                BadRequest(err.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task notificationUserFuntion(List<string> nameUsers, string dataRes)
        {
            try
            {
                var notificationHub = new NotificacionBrokerService();
                var connectionId = await notificationHub.getConnectionCliente(nameUsers!);

                if (connectionId.Any())
                {
                    connectionId.Select(id =>
                    {
                        _serviceNoti.Clients.Client(id).SendAsync("NotiService", dataRes);
                        return true;
                    }).ToList();
                }
            }
            catch (Exception err)
            {
                BadRequest(err.Message);
            }
        }


        public class SendNotification : DTO_QueueMessage
        {
            public string? dateSend { get; set; }
            public string? hour { get; set; }
        }

        [HttpGet]
        [Route("get-OldNotification")]
        public async Task<IActionResult> getNotifications(string nameUser)
        {
            try
            {
                List<DTO_QueueMessage> messageQueue =  _db.DTO_QueueMessage.Where(s => s.nameUser == nameUser && s.confirmRead == false).ToList();

                var SendNotification = messageQueue.Select(d => new SendNotification
                {
                    id = d.id,
                    userSend = d.userSend,
                    nameUser = d.nameUser,
                    message = d.message,
                    hour = d.registeNotification.ToString("t"),
                    dateSend = d.registeNotification.ToString("dd-MM-yyyy")
                }).ToList();

                return Ok(JsonSerializer.Serialize(SendNotification));
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        [Route("get-Notification")]
        public IActionResult getMessages(string nameUser) 
        {
            try
            {
                int messageQueue = _db.DTO_QueueMessage.Where(s => s.nameUser == nameUser && s.confirmRead == false).ToList().Count();

                return Ok(messageQueue);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }


        [HttpGet]
        [Route("clean-notification")]
        public async Task<IActionResult> cleanNoitification(int idNotification)
        {
            try
            {
                DTO_QueueMessage notiClean = _db.DTO_QueueMessage.FirstOrDefault(d => d.id == idNotification)!;

                notiClean.confirmRead = true;
                _db.DTO_QueueMessage.Update(notiClean);
                await _db.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }
    }
}

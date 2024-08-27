using BOM_centerNotification.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        public WeatherForecastController(IHttpClientFactory httpClient, IHubContext<NotificacionBrokerService> notificationBroker)
        {
            _httpClient = httpClient.CreateClient();
            _serviceNoti = notificationBroker;
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

        [HttpGet]
        [Route("Get-infoTarget")]
        public async Task<IActionResult> getTargetCredit(string sixDigit, string nameUser)
        {
            try
            {
                var endPoint = "https://data.handyapi.com/bin/" + sixDigit;
                var response = await _httpClient.GetAsync(endPoint);

                string dataRes = await response.Content.ReadAsStringAsync();
                var dataDes = JsonSerializer.Deserialize<DTO_targetInfo>(dataRes);

                //_serviceNoti.Clients.All.SendAsync("NotiService", dataRes);

                var notificationHub = new NotificacionBrokerService();
                var connectionId = await notificationHub.getConnectionCliente(nameUser);

                if(connectionId != null) _serviceNoti.Clients.Client(connectionId).SendAsync("NotiService", dataRes);

                return Ok(dataDes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

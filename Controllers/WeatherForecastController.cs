using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using WeatherAPI.Interfaces;
using WeatherAPI.Models;
using WeatherAPI.Repository;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly IConfiguration _conf;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly RabbitMQSender _rabbitmqSender;
        public WeatherForecastController(IWeatherRepository weatherRepository, IConfiguration conf)
        {
            _weatherRepository = weatherRepository;
            _conf = conf;
            _apiKey = _conf.GetSection("WeatherAPI").GetSection("WeatherApiKey").Value;
            _apiUrl = _conf.GetSection("WeatherAPI").GetSection("WeatherURL").Value;
            _rabbitmqSender = new("localhost", "audyt-logs", "guest", "guest","weather_audyt");
        }

        [HttpGet("GetCurrentWeather/{city}")]
        public async Task<IActionResult> Get([Required]string city)
        {
            try
            {
                AuditLogs auditLogs = new AuditLogs()
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    RequestedCity = city,
                    RequestedMethod = "GetCurrentWeather",
                    RequestTime = DateTime.Now,
                };
                _rabbitmqSender.SendMessage(System.Text.Json.JsonSerializer.Serialize(auditLogs));
                RestClientOptions restOptions = new RestClientOptions(_apiUrl);
                RestClient restClient = new RestClient(restOptions);
                RestRequest request = new($"v1/current.json?key={_apiKey}&q={city}");
                var response = await restClient.ExecuteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    await _weatherRepository.InsertWeather(response.Content);
                    Weather weather = System.Text.Json.JsonSerializer.Deserialize<Weather>(response.Content);
                    return new OkObjectResult(weather);
                }
                else
                {
                    return new NotFoundObjectResult("Nie znaleziono pogody");
                }
            }
            catch(Exception ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
        [HttpGet("GetFutureWeather/{city}/{date}")]
        public async Task<IActionResult> GetFutureWeather([Required]string city,[Required] string date)
        {
            try
            {
                AuditLogs auditLogs = new AuditLogs()
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    RequestedCity = city,
                    RequestedMethod = "GetFutureWeather",
                    RequestTime = DateTime.Now,
                };
                _rabbitmqSender.SendMessage(System.Text.Json.JsonSerializer.Serialize(auditLogs));
                RestClientOptions restOptions = new RestClientOptions(_apiUrl);
                RestClient restClient = new RestClient(restOptions);
                RestRequest request = new($"v1/future.json?q={city}&dt={date}&key={_apiKey}");
                var response = await restClient.ExecuteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _weatherRepository.InsertWeather(response.Content);
                    WeatherForecast weather = System.Text.Json.JsonSerializer.Deserialize<WeatherForecast>(response.Content);
                    return new OkObjectResult(weather);
                }
                else
                {
                    return new NotFoundObjectResult("Nie znaleziono pogody");
                }
            }
            catch (Exception ex)
            {

                return new NotFoundObjectResult(ex.Message);
            }
            
        }
        [HttpGet("GetForecast/{city}/{days}")]
        public async Task<IActionResult> GetForecast    ([Required] string city, [Required] string days)
        {
            try
            {
                AuditLogs auditLogs = new AuditLogs()
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    RequestedCity = city,
                    RequestedMethod = "GetForecastWeather",
                    RequestTime = DateTime.Now,
                };
                _rabbitmqSender.SendMessage(System.Text.Json.JsonSerializer.Serialize(auditLogs));
                RestClientOptions restOptions = new RestClientOptions(_apiUrl);
                RestClient restClient = new RestClient(restOptions);
                RestRequest request = new($"v1/forecast.json?q={city}&days={days}&key={_apiKey}");
                var response = await restClient.ExecuteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    WeatherForecast weather = System.Text.Json.JsonSerializer.Deserialize<WeatherForecast>(response.Content);
                    return new OkObjectResult(weather);
                }
                else
                {
                    return new NotFoundObjectResult("Nie znaleziono pogody");
                }
            }
            catch (Exception ex)
            {

                return new NotFoundObjectResult(ex.Message);
            }
            
        }
        [HttpGet("GetWeatherHistory/{city}/{date}")]
        public async Task<IActionResult> GetWeatherHistory(string city, string date)
        {
            try
            {
                AuditLogs auditLogs = new AuditLogs()
                {
                    IpAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    RequestedCity = city,
                    RequestedMethod = "GetWeatherHistory",
                    RequestTime = DateTime.Now,
                };
                _rabbitmqSender.SendMessage(System.Text.Json.JsonSerializer.Serialize(auditLogs));
                var weatherHistory = await _weatherRepository.GetWeather(city, DateTime.Parse(date));
                return new OkObjectResult(weatherHistory);
            }
            catch(Exception ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
    }
}

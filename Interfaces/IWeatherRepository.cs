using WeatherAPI.Models;

namespace WeatherAPI.Interfaces
{
    public interface IWeatherRepository
    {
        public Task<bool> InsertWeather(string json);
        public Task<Weather> GetWeather(string city, DateTime date);
        public Task<bool> InsertAuditLogs(string message);
    }
}

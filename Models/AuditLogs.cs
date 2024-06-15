namespace WeatherAPI.Models
{
    public class AuditLogs
    {
        public DateTime RequestTime { get; set; }
        public string IpAddress { get; set; }
        public string RequestedCity { get; set; }
        public string RequestedMethod { get; set; }
    }
}

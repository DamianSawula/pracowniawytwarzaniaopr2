using Dapper;
using Newtonsoft.Json;
using System.Data;
using WeatherAPI.Context;
using WeatherAPI.Interfaces;
using WeatherAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherAPI.Repository
{
    public class WeatherRepository :IWeatherRepository
    {
        private readonly DapperContext _context;
        public WeatherRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<bool> InsertWeather(string json)
        {
            try
            {
                var procedure = "dbo.InsertWeatherData";
                var parameters = new DynamicParameters();

                parameters.Add("@jsonData", json, DbType.String, direction: ParameterDirection.Input);
                using (var conn = _context.CreateConnection())
                {
                    try
                    {
                        var result = await conn.ExecuteAsync(procedure, parameters, commandType: CommandType.StoredProcedure);

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Błąd rejestrowania pogody w bazie.");
            }
        }
        public async Task<Weather> GetWeather(string city, DateTime date)
        {
            try
            {
                using (var conn = _context.CreateConnection())
                {

                    var result = await conn.QueryMultipleAsync(
                        "GetWeatherData",
                        new { LocationName = city, DateTime = date },
                        commandType: System.Data.CommandType.StoredProcedure
                    );

                    var location = await result.ReadSingleAsync<Location>();
                    var currentWeather = await result.ReadSingleAsync<Current>();
                    var condition = await result.ReadSingleAsync<Condition>();

                    currentWeather.condition = condition;

                    var weatherData = new Weather
                    {
                        location = location,
                        current = currentWeather
                    };

                    return weatherData;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Błąd pobierania historii pogody z bazy");
            }
        }
        public async Task<bool> InsertAuditLogs(string message)
        {
            try
            {
                var procedure = "dbo.InsertAuditLog";
                var parameters = new DynamicParameters();

                parameters.Add("@json", message, DbType.String, direction: ParameterDirection.Input);
                using (var conn = _context.CreateConnection())
                {
                    try
                    {
                        var result = await conn.ExecuteAsync(procedure, parameters, commandType: CommandType.StoredProcedure);

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Błąd rejestrowania pogody w bazie.");
            }
        }
    }
}

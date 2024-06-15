using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WeatherAPI.Models;

public class RabbitMQBackgroundService : BackgroundService
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;
    private readonly string _virtualHost;
    private readonly string _connectionString;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQBackgroundService(string hostname, string queueName, string username, string password, string virtualHost, string connectionString)
    {
        _hostname = hostname;
        _queueName = queueName;
        _username = username;
        _password = password;
        _virtualHost = virtualHost;
        _connectionString = connectionString;

        InitializeRabbitMqListener();
    }

    private void InitializeRabbitMqListener()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password,
            VirtualHost = _virtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            var auditLog = JsonSerializer.Deserialize<AuditLogs>(content);

            // Insert data into SQL Server
            await InsertAuditLog(auditLog);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_queueName, false, consumer);
        return Task.CompletedTask;
    }

    private async Task InsertAuditLog(AuditLogs auditLog)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("dbo.InsertAuditLog", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@json", JsonSerializer.Serialize(auditLog));
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}

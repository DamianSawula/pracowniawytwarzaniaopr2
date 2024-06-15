using System;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.Connections;

public class RabbitMQSender
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;
    private readonly string _VHost;

    public RabbitMQSender(string hostname, string queueName, string username, string password, string VHost)
    {
        _hostname = hostname;
        _queueName = queueName;
        _username = username;
        _password = password;
        _VHost = VHost;
    }

    public void SendMessage(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password,
            VirtualHost  = _VHost
            
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "audyt",
                                 routingKey: "audyt-logs",
                                 basicProperties: properties,
                                 body: body);

            Console.WriteLine($" [x] Sent '{message}'");
        }
    }
}

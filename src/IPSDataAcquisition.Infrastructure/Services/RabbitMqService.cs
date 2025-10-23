using IPSDataAcquisition.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace IPSDataAcquisition.Infrastructure.Services;

public class RabbitMqService : IMessageQueueService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new object();

    public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel != null && _channel.IsOpen)
            return _channel;

        var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.TryParse(_configuration["RabbitMQ:Port"], out var p) ? p : 5672;
        var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
        var password = _configuration["RabbitMQ:Password"] ?? "guest";
        var useSslConfig = _configuration["RabbitMQ:UseSsl"];
        var useSsl = bool.TryParse(useSslConfig, out var ssl) ? ssl : (port == 5671); // Default to true if port is 5671

        _logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}, SSL: {UseSsl}", hostName, port, useSsl);

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        // Enable SSL/TLS if required (for Amazon MQ)
        if (useSsl)
        {
            factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = hostName,
                AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch |
                                        System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors
            };
            _logger.LogInformation("SSL/TLS enabled for RabbitMQ connection");
        }

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        _logger.LogInformation("RabbitMQ connection established");

        return _channel;
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await GetChannelAsync();

            // Declare queue (idempotent - safe to call multiple times)
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body,
                mandatory: false,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Published message to queue: {QueueName}, Size: {Size} bytes", 
                queueName, body.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ queue: {QueueName}", queueName);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(string queueName, IEnumerable<T> messages, CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await GetChannelAsync();

            // Declare queue
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var messageList = messages.ToList();

            foreach (var message in messageList)
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    body: body,
                    mandatory: false,
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation("Published batch of {Count} messages to queue: {QueueName}", 
                messageList.Count, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing batch to RabbitMQ queue: {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            if (_channel != null)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
                _channel.Dispose();
            }
            
            if (_connection != null)
            {
                _connection.CloseAsync().GetAwaiter().GetResult();
                _connection.Dispose();
            }
            
            _logger.LogInformation("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}


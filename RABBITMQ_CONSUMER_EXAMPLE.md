# RabbitMQ Consumer - Background Worker Service

This is a template for creating a background service to consume IMU data from RabbitMQ and save to database.

## Create New Worker Project

```bash
# Create new worker service project
dotnet new worker -n IPSDataAcquisition.Worker
cd IPSDataAcquisition.Worker

# Add required packages
dotnet add package RabbitMQ.Client
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.Extensions.Hosting

# Add reference to existing projects
dotnet add reference ../src/IPSDataAcquisition.Infrastructure/IPSDataAcquisition.Infrastructure.csproj
dotnet add reference ../src/IPSDataAcquisition.Application/IPSDataAcquisition.Application.csproj
```

## Worker.cs Implementation

```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;

namespace IPSDataAcquisition.Worker;

public class IMUDataConsumerWorker : BackgroundService
{
    private readonly ILogger<IMUDataConsumerWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public IMUDataConsumerWorker(
        ILogger<IMUDataConsumerWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IMU Data Consumer Worker starting...");

        try
        {
            // Connect to RabbitMQ
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = int.TryParse(_configuration["RabbitMQ:Port"], out var p) ? p : 5672;
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare queue
            await _channel.QueueDeclareAsync(
                queue: "imu-data-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            // Set prefetch count (process 10 messages at a time)
            await _channel.BasicQosAsync(0, 10, false, stoppingToken);

            _logger.LogInformation("Connected to RabbitMQ. Waiting for messages...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("Received message from queue, Size: {Size} bytes", body.Length);

                    // Deserialize message
                    var queueMessage = JsonSerializer.Deserialize<IMUDataQueueMessage>(message);
                    
                    if (queueMessage == null || queueMessage.DataPoints == null)
                    {
                        _logger.LogWarning("Invalid message format, rejecting");
                        await _channel.BasicRejectAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    // Process message (save to database)
                    await ProcessIMUDataAsync(queueMessage, stoppingToken);

                    // Acknowledge message
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    
                    _logger.LogInformation("Successfully processed {Count} IMU data points for session {SessionId}",
                        queueMessage.DataPoints.Count, queueMessage.SessionId ?? "null");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject and requeue for retry
                    await _channel.BasicRejectAsync(ea.DeliveryTag, true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "imu-data-queue",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("IMU Data Consumer Worker stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in IMU Data Consumer Worker");
            throw;
        }
    }

    private async Task ProcessIMUDataAsync(IMUDataQueueMessage queueMessage, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var imuDataList = new List<IMUData>();

        foreach (var point in queueMessage.DataPoints)
        {
            var imuData = new IMUData
            {
                SessionId = queueMessage.SessionId,
                UserId = queueMessage.UserId,
                Timestamp = point.Timestamp,
                TimestampNanos = point.TimestampNanos,
                // Map all sensor fields...
                AccelX = point.AccelX, AccelY = point.AccelY, AccelZ = point.AccelZ,
                GyroX = point.GyroX, GyroY = point.GyroY, GyroZ = point.GyroZ,
                MagX = point.MagX, MagY = point.MagY, MagZ = point.MagZ,
                GravityX = point.GravityX, GravityY = point.GravityY, GravityZ = point.GravityZ,
                LinearAccelX = point.LinearAccelX, LinearAccelY = point.LinearAccelY, LinearAccelZ = point.LinearAccelZ,
                // ... (map remaining 50+ fields)
                Latitude = point.Latitude, Longitude = point.Longitude, Altitude = point.Altitude,
                GpsAccuracy = point.GpsAccuracy, Speed = point.Speed,
                IsSynced = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            imuDataList.Add(imuData);
        }

        // Bulk insert
        await context.IMUData.AddRangeAsync(imuDataList, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved {Count} IMU data points to database", imuDataList.Count);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping IMU Data Consumer Worker...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(stoppingToken);
    }
}

// Message format (must match PublishIMUDataToQueueCommandHandler)
public class IMUDataQueueMessage
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public List<IMUDataPointDto> DataPoints { get; set; } = new();
    public DateTime ReceivedAt { get; set; }
}
```

## Program.cs

```csharp
using IPSDataAcquisition.Infrastructure;
using IPSDataAcquisition.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Add Infrastructure services (DbContext, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add background worker
builder.Services.AddHostedService<IMUDataConsumerWorker>();

var host = builder.Build();
host.Run();
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ips_data;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Run Worker

```bash
# Development
dotnet run --project IPSDataAcquisition.Worker

# Production (as systemd service)
sudo systemctl enable ips-worker
sudo systemctl start ips-worker
```

## Deploy Worker to AWS

### Option 1: ECS Task (Recommended)
```bash
# Create task definition for worker
aws ecs register-task-definition --cli-input-json file://worker-task-definition.json

# Run as ECS service
aws ecs create-service \
  --cluster ips-api-cluster \
  --service-name ips-worker-service \
  --task-definition ips-worker:1 \
  --desired-count 2
```

### Option 2: EC2 Background Process
```bash
# Run worker in Docker on same EC2
docker run -d \
  --name ips-worker \
  --network host \
  -e ConnectionStrings__Default="..." \
  -e RabbitMQ__HostName="localhost" \
  --restart unless-stopped \
  your-ecr-registry/ips-worker:latest
```

## Benefits

✅ **Decoupled Processing**: API responds immediately, processing happens async
✅ **Scalability**: Scale workers independently (2-10 instances)
✅ **Resilience**: If worker crashes, messages stay in queue
✅ **Retry Logic**: Failed messages automatically requeued
✅ **Monitoring**: RabbitMQ UI shows queue depth, processing rate
✅ **Backpressure**: QoS prevents worker overload


using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.IMUData.Commands.PublishIMUDataToQueue;

public class PublishIMUDataToQueueCommandHandler : IRequestHandler<PublishIMUDataToQueueCommand, UploadIMUDataResponseDto>
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PublishIMUDataToQueueCommandHandler> _logger;

    public PublishIMUDataToQueueCommandHandler(
        IMessageQueueService messageQueueService,
        ICurrentUserService currentUserService,
        ILogger<PublishIMUDataToQueueCommandHandler> logger)
    {
        _messageQueueService = messageQueueService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UploadIMUDataResponseDto> Handle(PublishIMUDataToQueueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing IMU data to queue: {Count} data points for session {SessionId}, User: {UserId}", 
            request.DataPoints.Count, request.SessionId ?? "null", _currentUserService.UserId);

        // Create message payload
        var queueMessage = new IMUDataQueueMessage
        {
            SessionId = request.SessionId,
            UserId = _currentUserService.UserId,
            DataPoints = request.DataPoints,
            ReceivedAt = DateTime.UtcNow
        };

        try
        {
            // Publish to RabbitMQ
            await _messageQueueService.PublishAsync("imu-data-queue", queueMessage, cancellationToken);

            _logger.LogInformation("Successfully published {Count} IMU data points to queue for session {SessionId}", 
                request.DataPoints.Count, request.SessionId ?? "null");

            return new UploadIMUDataResponseDto(request.DataPoints.Count, request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish IMU data to queue for session {SessionId}", request.SessionId);
            throw;
        }
    }
}

// Message format for RabbitMQ queue
public record IMUDataQueueMessage
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public List<IMUDataPointDto> DataPoints { get; set; } = new();
    public DateTime ReceivedAt { get; set; }
}


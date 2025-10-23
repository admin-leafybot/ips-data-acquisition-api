namespace IPSDataAcquisition.Application.Common.Interfaces;

public interface IMessageQueueService
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
    Task PublishBatchAsync<T>(string queueName, IEnumerable<T> messages, CancellationToken cancellationToken = default);
}


namespace Orders.Infrastructure.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken);
}

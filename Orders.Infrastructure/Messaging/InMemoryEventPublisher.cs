using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Abstractions;

namespace Orders.Infrastructure.Messaging;

public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("In-memory publish {EventType}: {Payload}", eventType, payload);
        return Task.CompletedTask;
    }
}

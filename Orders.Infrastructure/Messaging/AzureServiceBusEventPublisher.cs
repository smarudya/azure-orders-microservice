using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Abstractions;
using System.Text;

namespace Orders.Infrastructure.Messaging;

public sealed class AzureServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly string _topicName;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;

    public AzureServiceBusEventPublisher(ServiceBusClient client, string topicName, ILogger<AzureServiceBusEventPublisher> logger)
    {
        _client = client;
        _topicName = topicName;
        _logger = logger;
    }

    public async Task PublishAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        var sender = _client.CreateSender(_topicName);
        await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(payload))
        {
            Subject = eventType
        }, cancellationToken);
        await sender.DisposeAsync();
        _logger.LogInformation("Published event {EventType} to Azure Service Bus topic {Topic}.", eventType, _topicName);
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}

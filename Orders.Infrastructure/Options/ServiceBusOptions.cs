namespace Orders.Infrastructure.Options;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";
    public string? ConnectionString { get; set; }
    public string TopicName { get; set; } = "orders-events";
}

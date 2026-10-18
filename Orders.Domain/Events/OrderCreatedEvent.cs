namespace Orders.Domain.Events;

public sealed record OrderCreatedEvent(Guid OrderId, Guid CustomerId, DateTimeOffset CreatedAtUtc);

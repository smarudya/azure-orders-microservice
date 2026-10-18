namespace Orders.Api.Contracts;

public sealed class CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public required IReadOnlyCollection<CreateOrderItemRequest> Items { get; init; }
}

public sealed class CreateOrderItemRequest
{
    public required string Sku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

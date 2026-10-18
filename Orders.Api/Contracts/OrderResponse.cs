using Orders.Domain.Entities;

namespace Orders.Api.Contracts;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items)
{
    public static OrderResponse FromDomain(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.Items.Select(x => new OrderItemResponse(x.Sku, x.Quantity, x.UnitPrice)).ToList());
    }
}

public sealed record OrderItemResponse(string Sku, int Quantity, decimal UnitPrice);

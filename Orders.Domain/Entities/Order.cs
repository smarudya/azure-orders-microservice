using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastUpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order()
    {
    }

    private Order(Guid customerId, IEnumerable<OrderItem> items)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId must be a valid GUID.", nameof(customerId));
        }

        CustomerId = customerId;
        AddItems(items);
    }

    public static Order Create(Guid customerId, IEnumerable<OrderItem> items)
    {
        return new Order(customerId, items);
    }

    public decimal TotalAmount => _items.Sum(x => x.UnitPrice * x.Quantity);

    public void MarkPaid()
    {
        if (Status != OrderStatus.Created)
        {
            throw new DomainRuleViolationException($"Cannot mark order as paid from {Status}.");
        }

        Status = OrderStatus.Paid;
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkShipped()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new DomainRuleViolationException("Order must be paid before shipping.");
        }

        Status = OrderStatus.Shipped;
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
        {
            throw new DomainRuleViolationException("Shipped order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private void AddItems(IEnumerable<OrderItem> items)
    {
        var materialized = items?.ToList() ?? [];
        if (materialized.Count == 0)
        {
            throw new DomainRuleViolationException("Order must contain at least one item.");
        }

        _items.AddRange(materialized);
    }
}

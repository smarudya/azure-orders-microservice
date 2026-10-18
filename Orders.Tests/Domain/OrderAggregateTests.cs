using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Exceptions;

namespace Orders.Tests.Domain;

public sealed class OrderAggregateTests
{
    [Fact]
    public void Create_Computes_Total_And_Default_Status()
    {
        var order = Order.Create(Guid.NewGuid(), new[]
        {
            new OrderItem("SKU-1", 2, 10m),
            new OrderItem("SKU-2", 1, 5m)
        });

        Assert.Equal(25m, order.TotalAmount);
        Assert.Equal(OrderStatus.Created, order.Status);
    }

    [Fact]
    public void MarkShipped_Without_Paid_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { new OrderItem("SKU-1", 1, 10m) });
        Assert.Throws<DomainRuleViolationException>(() => order.MarkShipped());
    }

    [Fact]
    public void Cancel_After_Shipped_Throws()
    {
        var order = Order.Create(Guid.NewGuid(), new[] { new OrderItem("SKU-1", 1, 10m) });
        order.MarkPaid();
        order.MarkShipped();
        Assert.Throws<DomainRuleViolationException>(() => order.Cancel());
    }
}

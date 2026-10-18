using Microsoft.EntityFrameworkCore;
using Orders.Api.Contracts;
using Orders.Domain.Entities;
using Orders.Domain.Events;
using Orders.Infrastructure.Abstractions;
using Orders.Infrastructure.Data;
using System.Text.Json;

namespace Orders.Api.Services;

public sealed class OrderCommandService
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrdersDbContext _dbContext;

    public OrderCommandService(IOrderRepository orderRepository, OrdersDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _dbContext = dbContext;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(x => new OrderItem(x.Sku, x.Quantity, x.UnitPrice));
        var order = Order.Create(request.CustomerId, items);

        // InMemory provider (tests) has no relational transactions; SQLite/SQL do.
        if (_dbContext.Database.IsRelational())
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await PersistOrderWithOutboxAsync(order, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await PersistOrderWithOutboxAsync(order, cancellationToken);
        }

        return order;
    }

    private async Task PersistOrderWithOutboxAsync(Order order, CancellationToken cancellationToken)
    {
        await _orderRepository.AddAsync(order, cancellationToken);

        var domainEvent = new OrderCreatedEvent(order.Id, order.CustomerId, order.CreatedAtUtc);
        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            EventType = nameof(OrderCreatedEvent),
            Payload = JsonSerializer.Serialize(domainEvent),
            OccurredAtUtc = DateTimeOffset.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

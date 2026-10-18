using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Infrastructure.Abstractions;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> ListAsync(OrderStatus? status, int take, CancellationToken cancellationToken);
}

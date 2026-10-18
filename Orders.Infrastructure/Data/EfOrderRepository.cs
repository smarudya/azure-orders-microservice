using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Infrastructure.Abstractions;

namespace Orders.Infrastructure.Data;

public sealed class EfOrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public EfOrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> ListAsync(OrderStatus? status, int take, CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders.Include(x => x.Items).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}

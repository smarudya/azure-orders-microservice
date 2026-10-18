using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Abstractions;
using Orders.Infrastructure.Data;

namespace Orders.Infrastructure.Messaging;

public sealed class OutboxDispatcher
{
    private readonly OrdersDbContext _dbContext;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(OrdersDbContext dbContext, IEventPublisher eventPublisher, ILogger<OutboxDispatcher> logger)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<int> DispatchPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pending = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                await _eventPublisher.PublishAsync(message.EventType, message.Payload, cancellationToken);
                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed publishing outbox message {OutboxMessageId}", message.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }
}

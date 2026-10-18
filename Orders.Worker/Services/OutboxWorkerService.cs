using Orders.Infrastructure.Messaging;

namespace Orders.Worker.Services;

public sealed class OutboxWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorkerService> _logger;

    public OutboxWorkerService(IServiceScopeFactory scopeFactory, ILogger<OutboxWorkerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
                var processed = await dispatcher.DispatchPendingAsync(25, stoppingToken);

                if (processed > 0)
                {
                    _logger.LogInformation("Processed {Count} outbox messages.", processed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox worker loop failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}

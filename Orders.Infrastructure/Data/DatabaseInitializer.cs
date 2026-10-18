using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Orders.Infrastructure.Data;

public sealed class DatabaseInitializer
{
    private readonly OrdersDbContext _dbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(OrdersDbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        _logger.LogInformation("Orders database is ready.");
    }
}

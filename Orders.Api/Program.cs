using OpenTelemetry.Trace;
using Orders.Api.Contracts;
using Orders.Api.Services;
using Orders.Domain.Enums;
using Orders.Infrastructure;
using Orders.Infrastructure.Abstractions;
using Orders.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrdersDbContext>("orders-db");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
    });

builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddScoped<OrderCommandService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync(CancellationToken.None);
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapHealthChecks("/ready");

app.MapPost("/orders", async (CreateOrderRequest request, OrderCommandService commandService, CancellationToken ct) =>
{
    var created = await commandService.CreateOrderAsync(request, ct);
    return Results.Created($"/orders/{created.Id}", OrderResponse.FromDomain(created));
});

app.MapGet("/orders/{id:guid}", async (Guid id, IOrderRepository repository, CancellationToken ct) =>
{
    var order = await repository.GetByIdAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(OrderResponse.FromDomain(order));
});

app.MapGet("/orders", async (string? status, int? take, IOrderRepository repository, CancellationToken ct) =>
{
    OrderStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var result))
    {
        parsedStatus = result;
    }

    var orders = await repository.ListAsync(parsedStatus, Math.Clamp(take ?? 50, 1, 200), ct);
    return Results.Ok(orders.Select(OrderResponse.FromDomain));
});

app.Run();

public partial class Program;

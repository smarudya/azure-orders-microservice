using OpenTelemetry.Trace;
using Orders.Infrastructure;
using Orders.Infrastructure.Data;
using Orders.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddHttpClientInstrumentation();
    });

builder.Services.AddOrdersInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OutboxWorkerService>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync(CancellationToken.None);
}

host.Run();

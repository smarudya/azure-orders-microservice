using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Abstractions;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Options;

namespace Orders.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdersDb") ?? "Data Source=orders.db";
        services.AddDbContext<OrdersDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<OutboxDispatcher>();
        services.AddScoped<DatabaseInitializer>();

        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));
        var serviceBusConnection = configuration[$"{ServiceBusOptions.SectionName}:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(serviceBusConnection))
        {
            var topic = configuration[$"{ServiceBusOptions.SectionName}:TopicName"] ?? "orders-events";
            services.AddSingleton(new ServiceBusClient(serviceBusConnection));
            services.AddSingleton<IEventPublisher>(sp =>
                new AzureServiceBusEventPublisher(
                    sp.GetRequiredService<ServiceBusClient>(),
                    topic,
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AzureServiceBusEventPublisher>>()));
        }
        else
        {
            services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
        }

        return services;
    }
}

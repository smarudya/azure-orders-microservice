using Orders.Api.Contracts;
using System.Net.Http.Json;

namespace Orders.Tests.Integration;

public sealed class OrdersApiTests : IClassFixture<OrdersApiFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(OrdersApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Order_Then_Get_By_Id_Works()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new CreateOrderItemRequest { Sku = "SKU-1", Quantity = 2, UnitPrice = 9.99m }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/orders", request);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/orders/{created!.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Created", fetched.Status);
    }
}

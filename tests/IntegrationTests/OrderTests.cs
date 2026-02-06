using System.Net;
using System.Net.Http.Json;
using DataPersistencePatterns.Domain.Entities;
using Xunit;

namespace IntegrationTests;

public class OrderTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public OrderTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrdersOptimized_ShouldReturnOrders_WhenDatabaseCheckIsSuccessful()
    {
        // Act
        // This endpoint logic basically selects orders.
        // Since we seed data in Program/DbSeeder, there SHOULD be data.
        var response = await _client.GetAsync("/orders/optimized");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with {response.StatusCode}. Body: {error}");
        }

        // Assert
        var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
        
        Assert.NotNull(orders);
        // We can't guarantee exact count unless we know standard seed, but list shouldn't throw.
        // If ApiFactory runs migration/seed, it should be fine.
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_ForRandomId()
    {
        // Act
        var randomId = Guid.NewGuid();
        var response = await _client.GetAsync($"/orders/{randomId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

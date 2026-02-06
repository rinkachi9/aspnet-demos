using AdvancedEntityFramework.Data;
using AdvancedEntityFramework.Entities;
using AdvancedEntityFramework.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace AdvancedTesting.Integration;

public class PostgresIntegrationTest : IAsyncLifetime
{
    // Define the container
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private OptimizationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OptimizationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var context = new OptimizationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task OrderService_Should_Create_Order_With_Outbox_In_Transaction()
    {
        // Arrange
        using var context = CreateContext();
        var service = new OrderService(context);
        var customerName = "Integration-Test-Customer";
        var amount = 123.45m;

        // Act
        await service.PlaceOrderWithOutboxAsync(customerName, amount);

        // Assert
        using var verifyContext = CreateContext();
        
        var order = await verifyContext.Orders.FirstOrDefaultAsync(o => o.CustomerName == customerName);
        order.Should().NotBeNull();
        order!.Version.Should().NotBe(0); // Version should be set by xmin (or shim)

        var outboxMessage = await verifyContext.Outbox.FirstOrDefaultAsync(m => m.Type == "OrderCreated");
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Content.Should().Contain(amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}

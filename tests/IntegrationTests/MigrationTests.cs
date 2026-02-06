using AdvancedMigrations.Services;
using DataPersistencePatterns.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests;

public class MigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Register DbContext with Testcontainer
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_container.GetConnectionString(), npgsql =>
            {
                npgsql.MigrationsAssembly("DataPersistencePatterns");
                npgsql.UseNodaTime();
            })
            .UseSnakeCaseNamingConvention());

        services.AddLogging(logging => logging.AddConsole());
        services.AddTransient<MigratorService>();
        services.AddTransient<SeederService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Safe_Migration_Rollback_And_Seeding_Flow()
    {
        var services = CreateServiceProvider();
        var migrator = services.GetRequiredService<MigratorService>();
        var seeder = services.GetRequiredService<SeederService>();
        var db = services.GetRequiredService<AppDbContext>();

        // 1. Migrate Up
        await migrator.MigrateAsync(CancellationToken.None);
        
        // Verify Tables Exist
        var pending = await db.Database.GetPendingMigrationsAsync();
        Assert.Empty(pending);

        // 2. Seed Data
        await seeder.SeedAsync("IntegrationTest", "performance", CancellationToken.None);
        
        // Verify Data
        var count = await db.Orders.CountAsync();
        Assert.True(count >= 1, "Orders should be seeded");
        
        var perfOrder = await db.Orders.AnyAsync(o => o.CustomerName == "Perf Customer");
        Assert.True(perfOrder, "Performance Tag should seed specific customer");

        // 3. Rollback (to 0 / Empty)
        // "0" is the magic string in EF Core to rollback all migrations
        await migrator.RollbackAsync("0", CancellationToken.None);

        // Verify Schema is gone (Querying orders should throw or return false if table missing)
        // Actually, checking applied migrations is safer.
        // We need to re-create context or clear cache if model changed significantly, 
        // but here we just check migrations table.
        var applied = await db.Database.GetAppliedMigrationsAsync();
        Assert.Empty(applied);
    }
}

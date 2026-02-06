using DataPersistencePatterns.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // Enable validation and detailed errors
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // Register default Npgsql options with NodaTime because the app expects it
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), npgsqlOptions =>
                {
                    npgsqlOptions.UseNodaTime();
                })
                .UseSnakeCaseNamingConvention();
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}

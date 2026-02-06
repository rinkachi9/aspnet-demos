using Microsoft.EntityFrameworkCore;
using AdvancedTuning.Data;

namespace AdvancedTuning.Networking;

public static class PoolingConfig
{
    public static void ConfigurePooling(IServiceCollection services, IConfiguration configuration)
    {
        // HTTP CLIENT POOLING
        services.AddHttpClient("PooledClient", client => 
        {
            client.BaseAddress = new Uri("https://example.com");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 100 
        });

        // DATABASE POOLING
        // AddDbContextPool registers dependencies as scoped, 
        // but the Context instance itself is kept in a Singleton pool.
        // On request end, it is RESET (state cleared) and returned to pool.
        services.AddDbContextPool<TuningDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        }, poolSize: 128); // Default is 1024, adjust based on heavy load
    }
}

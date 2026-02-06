using AdvancedCaching.Services;
using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace AdvancedCaching;

public static class CacheExtensions
{
    public static IHostApplicationBuilder AddAdvancedCaching(this IHostApplicationBuilder builder)
    {
        // 1. Redis Configuration
        var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var muxer = ConnectionMultiplexer.Connect(redisConn);
        builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);

        // 2. Distributed Cache (L2)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "AdvancedCaching_";
        });

        // 3. HybridCache (L1 + L2)
        // HybridCache uses IDistributedCache by default for L2.
        builder.Services.AddHybridCache();

        // 4. Backplane (Custom)
        builder.Services.AddSingleton<ICacheInvalidator, RedisCacheInvalidator>();
        builder.Services.AddHostedService<CacheInvalidationService>();

        // 5. Output Cache
        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
            options.AddPolicy("NoCache", builder => builder.NoCache());
            options.AddPolicy("Expire20", builder => builder.Expire(TimeSpan.FromSeconds(20)).Tag("articles"));
        });
        
        return builder;
    }
}

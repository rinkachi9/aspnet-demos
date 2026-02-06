using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ApiDesignPatterns.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "POST" && context.Request.Method != "PATCH")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"Idempotency:{idempotencyKey}";
        var cachedResponse = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            await _cache.SetStringAsync(cacheKey, responseText, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            await responseBody.CopyToAsync(originalBodyStream);
        }
        else 
        {
             // Copy failed response back but don't cache
             responseBody.Seek(0, SeekOrigin.Begin);
             await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}

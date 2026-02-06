# ResiliencePatterns

Demonstrates high-performance and fault-tolerant patterns using Redis, Polly, and .NET Core features.

## Key Concepts
1.  **Distributed Output Caching**: Uses `StackExchange.Redis` to store cached responses.
    - Configuration: `AddStackExchangeRedisOutputCache`.
    - Usage: `.CacheOutput("PolicyName")` endpoint metadata.
2.  **Rate Limiting**: Built-in .NET 7+ Rate Limiting middleware.
    - **Global**: Per IP Fixed Window.
    - **Strict Policy**: Token Bucket algorithm (3 tokens max, refills 1 every 10s).
3.  **Resilience (Polly)**:
    - **Standard Resilience Handler**: Includes Retry, Circuit Breaker, Timeout, and Hedging (optional).
    - Applied to `HttpClient` consuming an "Unstable" service.

## Infrastructure
Redis is required. Run:
```bash
docker compose -f docker/docker-compose.yml up -d
```

## How to Test
1.  **Caching**:
    - `GET /weather?city=Warsaw` -> First call ~200ms (Simulated delay).
    - `GET /weather?city=Warsaw` -> Second call ~0ms (Cached).
2.  **Resilience**:
    - Check logs when hitting `/weather`. Even if the internal logic throws, Polly might retry (look for warnings).
3.  **Rate Limiting**:
    - `GET /limited` -> Call 4 times within 10 seconds.
    - 4th call should return `429 Too Many Requests`.

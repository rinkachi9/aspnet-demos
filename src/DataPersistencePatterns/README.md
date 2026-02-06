# DataPersistencePatterns

Demonstrates high-performance data access using **EF Core 8** with **PostgreSQL**.

## Key Concepts
1.  **NodaTime Integration**: `Instant` maps to PostgreSQL `timestamp with time zone`.
2.  **Naming Conventions**: `UseSnakeCaseNamingConvention` maps `CustomerName` to `customer_name`.
3.  **Optimizations**:
    - `AsNoTracking()`: Disables change tracking for read-only scenarios.
    - `AsSplitQuery()`: Splits 1:N queries into separate SQL commands to avoid data explosion.
    - `CompiledQueries`: Caches query plans for frequently executed lookups.
4.  **Resilience**: `EnableRetryOnFailure()` configured in Npgsql provider.

## Infrastructure
Postgres required.
```bash
docker compose -f docker/docker-compose.yml up -d
```
Connection: `Host=localhost;Database=advanced_playground;Username=admin;Password=password`

## How to Test
1.  **Swagger**: `http://localhost:5005/swagger`
2.  **Optimized Query**: `GET /orders/optimized`
    - Check logs to see `SPLIT QUERY` behavior (multiple SELECTs).
3.  **Compiled Query**: `GET /orders/compiled/50`
    - Executes pre-compiled delegate avoiding overhead.

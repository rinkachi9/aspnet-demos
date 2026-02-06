# ASP.NET Core Advanced Playground

A comprehensive portfolio of 12 projects demonstrating advanced ASP.NET Core patterns, architecture, and engineering practices (targeting .NET 9.0).

## Projects Overview

### 1. Core & Architecture
- **[MinimalApiPipeline](src/MinimalApiPipeline)**: Advanced Minimal APIs, Middleware, and Global Error Handling.
- **[ArchitecturePatterns](src/ArchitecturePatterns)**: Vertical Slice Architecture, DDD, MediatR, and Domain Events.
- **[ApiDesignPatterns](src/ApiDesignPatterns)**: API Versioning using `Asp.Versioning`, Idempotency Middleware, and Pagination.

### 2. Data & Messaging
- **[DataPersistencePatterns](src/DataPersistencePatterns)**: EF Core 8/9, Postgres, Compiled Queries, Split Queries, and Optimizations.
- **[MessagingPatterns](src/MessagingPatterns)**: Asynchronous Messaging with MassTransit, RabbitMQ, Sagas, and State Machines.
- **[AdvancedServicePatterns](src/AdvancedServicePatterns)**: (Legacy) Advanced DI (Scrutor, Decorators).
- **[BackgroundJobsQuartz](src/BackgroundJobsQuartz)**: Native Quartz.NET with Auto-Discovery, Attributes, and Listeners.
- **[BackgroundJobsHangfire](src/BackgroundJobsHangfire)**: Hangfire with Postgres, Dashboard, and Custom Filters.

### 3. Stability & Security
- **[ResiliencePatterns](src/ResiliencePatterns)**: Redis Output Caching, Rate Limiting, and Polly Policies (Retry, Circuit Breaker).
- **[SecurityPatterns](src/SecurityPatterns)**: OAuth2/OIDC with Keycloak, JWT Bearer, Policy-based Auth, and Security Headers.

### 4. Operations & Observability
- **[ObservabilityPatterns](src/ObservabilityPatterns)**: OpenTelemetry (Tracing, Metrics, Logging) with Jaeger, Prometheus, and Grafana.
- **[LocalizationPatterns](src/LocalizationPatterns)**: Internationalization (I18n) and Content Negotiation.
- **[DevOps](src/DevOps)**: Multi-stage Dockerfiles (Alpine) and GitHub Actions CI workflow.
- **[PerformancePatterns](src/PerformancePatterns)**: Benchmarking (`BenchmarkDotNet`) for JSON and String operations.

## Technologies Used
- **Framework**: .NET 9.0 (ASP.NET Core)
- **Database**: PostgreSQL
- **Message Broker**: RabbitMQ
- **Caching**: Redis
- **Auth**: Keycloak
- **Observability**: Jaeger, Prometheus, Grafana
- **Libraries**: MassTransit, MediatR, FluentValidation, Polly, OpenTelemetry, Serilog, Scrutor, BenchmarkDotNet.

## How to Run
Prerequisites: Docker & Docker Compose.

1.  **Start Infrastructure**:
    ```bash
    docker compose -f docker/docker-compose.yml up -d
    ```
2.  **Build Solution**:
    ```bash
    dotnet build
    ```
3.  **Run Specific Project**:
    ```bash
    dotnet run --project src/MinimalApiPipeline/MinimalApiPipeline.csproj
    ```

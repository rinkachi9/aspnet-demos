# ObservabilityPatterns

Demonstrates **OpenTelemetry** integration for Tracing, Metrics, and Logging.

## Key Concepts
1.  **OpenTelemetry Tracing**: Captures distributed traces (Spans).
    - Auto-instrumentation for ASP.NET Core & HttpClient.
    - Manual `ActivitySource` for business logic blocks.
2.  **OpenTelemetry Metrics**: Captures runtime numbers.
    - Exported to Prometheus.
3.  **OpenTelemetry Logging**: exports logs via OTLP.

## Infrastructure
- **Jaeger**: Tracing trace (http://localhost:16686).
- **Prometheus**: Metrics (http://localhost:9090).
- **Grafana**: Dashboard (http://localhost:3000).

```bash
docker compose -f docker/docker-compose.yml up -d
```

## How to Test
1.  **Swagger**: `http://localhost:5007/swagger`
2.  **Generate Traces**:
    - Call `GET /work` multiple times.
    - Call `GET /error` to see error tracking.
3.  **View Traces**: Open Jaeger UI.
4.  **View Metrics**: Open Prometheus and query `work_operations_total`.

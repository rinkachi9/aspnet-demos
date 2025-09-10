# dotnet-opentelemetry-kafka

ASP.NET 8 API + .NET 8 Worker with Kafka (Redpanda) and full OpenTelemetry (traces, metrics, logs). 
Traces -> Jaeger via OTel Collector; Metrics -> Prometheus (scrape /metrics); Logs -> Loki (viewed in Grafana).

## Stack

- **API** (`src/Api`) – ASP.NET 8 minimal API, publishes messages to Kafka on `POST /publish` with string body payload.
- **Worker** (`src/Worker`) – KafkaFlow consumer that processes messages and emits telemetry.
- **Redpanda** – Kafka-compatible broker (single node).
- **OpenTelemetry Collector** – receives OTLP (traces, logs) and exports to Jaeger/Loki.
- **Jaeger** – traces UI at http://localhost:16686
- **Prometheus** – metrics at http://localhost:9090
- **Grafana** – dashboards/logs at http://localhost:3000 (admin/admin)
- **Loki** – logs backend for Grafana.

## Run

```bash
docker compose up --build
```

Then publish a test message:

```bash
curl -X POST http://localhost:8080/publish -H "Content-Type: application/json" -d '"hello from api"'
```

## Observe

- **Traces**: open Jaeger at http://localhost:16686 and search for services `demo.api` and `demo.worker`.
- **Metrics**: Prometheus scrapes `/metrics` from both services. Add Grafana panel targeting Prometheus.
- **Logs**: Open Grafana -> Explore -> select *Loki* and query `{service="demo.api"}` or `{service="demo.worker"}`.

## Notes

- OTLP endpoint is `http://otel-collector:4317` (set via env).
- Prometheus exporter is the ASP.NET Prometheus exporter: `/metrics` on each service port.
- Trace context is propagated via Kafka message headers using W3C context; the worker extracts it to create consumer spans.

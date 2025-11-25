# dotnet-opentelemetry-kafka

ASP.NET 8 API + .NET 8 Worker with Kafka (Redpanda) and full OpenTelemetry (traces, metrics, logs).
Traces -> Jaeger via OTel Collector; Metrics -> Prometheus (scrape `/metrics`); Logs -> Loki (viewed in Grafana).

## Stack

- **API** (`Demo.Api`) – ASP.NET Core API with controllers. Publishes messages to Kafka via `POST /api/messages` (alias kept at `POST /publish`).
- **Worker** (`Demo.Worker`) – KafkaFlow consumer (typed handler) with manual Kafka trace propagation and custom metrics for processing latency.
- **Redpanda** – Kafka-compatible broker (single node).
- **OpenTelemetry Collector** – receives OTLP (traces, logs, metrics) and exports to Jaeger/Loki.
- **Jaeger** – traces UI at http://localhost:16686
- **Prometheus** – metrics at http://localhost:9090
- **Grafana** – dashboards/logs at http://localhost:3000 (admin/admin)
- **Loki** – logs backend for Grafana.

## Run

```bash
docker compose up
```

Publish a test message:

```bash
curl -X POST http://localhost:8080/api/messages \
  -H "Content-Type: application/json" \
  -d '{"payload":"hello from api"}'
```

## Observe

- **Traces**: open Jaeger at http://localhost:16686 and search for services `demo.api` and `demo.worker`.
- **Metrics**: Prometheus scrapes `/metrics` from both services. Custom histogram `demo.message.processing.duration` records worker processing time.
- **Logs**: Open Grafana -> Explore -> select *Loki* and query `{service="demo.api"}` or `{service="demo.worker"}`.

## Configuration

- `Demo.Api/appsettings.json` and `Demo.Worker/appsettings.json` contain Kafka (`Kafka:Brokers`, `Kafka:ConsumerGroupId`) and OTLP endpoint (`OpenTelemetry:OtlpEndpoint`) defaults. Override via env vars as needed.
- Trace context and baggage are propagated over Kafka headers (W3C). The worker extracts them to continue the trace and attaches messaging tags/metrics.

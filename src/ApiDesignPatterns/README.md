# ApiDesignPatterns

Demonstrates advanced REST API patterns: **Versioning**, **Idempotency**, and **Pagination**.

## Key Concepts
1.  **API Versioning**:
    - `URL` and `Header` strategies.
2.  **Advanced Middleware**:
    - **Global Exception Handling**: Returns `ProblemDetails`.
    - **Correlation ID**: Tracks requests across logs.
    - **Pipeline Branching**: `MapWhen` (Admin) and `UseWhen` (Conditional Log).
3.  **Content Negotiation**:
    - Supports `application/json` (Default).
    - Supports `application/x-protobuf` (High performance) via `protobuf-net`.
    - Supports `application/xml`.
4.  **Optimization**:
    - Response Compression (Brotli).
    - Idempotency for POST requests.

## How to Test
1.  **Swagger**: `http://localhost:5008/swagger`
2.  **Protobuf**:
    - Endpoint: `GET /api/v1/minimal-proto`
    - Header: `Accept: application/x-protobuf`
    - Tool: Postman/Insomnia (Browser will download binary).
3.  **Admin Pipeline**:
    - Endpoint: `GET /admin/anything`
    - Result: "Admin Area - Isolated Pipeline Branch".

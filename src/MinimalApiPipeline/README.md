# MinimalApiPipeline

This project demonstrates core concepts of a production-grade Minimal API in ASP.NET Core 8.0/9.0.

## Key Concepts Demonstrated

1.  **Route Groups**: Organizing endpoints using `MapGroup` for better structure and applying common conventions (prefixes, tags).
2.  **TypedResults**: Using `TypedResults` helper for type-safe, metadata-rich responses that integrate automatically with OpenAPI.
3.  **Endpoint Filters**: "Cross-cutting concerns" for endpoints (validation, logging) applied via `AddEndpointFilter`.
4.  **Middleware**:
    - Custom global Exception Handling Middleware.
    - Request/Response Pipeline branching (`MapWhen`/`UseWhen`).
    - Standard Middleware: Compression, Content Negotiation.
5.  **Validation**: Separation of validation logic using FluentValidation.
6.  **OpenAPI**: Swagger UI generation without Controllers, using metadata from Route Groups and TypedResults.

## Project Structure

- `Endpoints/`: Contains static classes defining route groups (e.g., `UserEndpoints`).
- `Filters/`: Custom `IEndpointFilter` implementations.
- `Middleware/`: Custom Middleware classes.
- `Models/`: DTOs and Domain models.
- `Validations/`: FluentValidation validators.

## How to Run

```bash
dotnet run
```

Endpoints to try:
- `POST /api/users` - Creates a user (Validates input).
- `GET /api/users/{id}` - Retrieves a user.
- `GET /admin/stats` - Protected/Branched pipeline example.

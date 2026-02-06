# Clean Architecture + MediatR

A strict implementation of **Clean Architecture** (Onion Architecture) separation of concerns.

## Dependency Rules
1.  **Domain**: Core Business Logic. No dependencies.
2.  **Application**: Use Cases & CQRS. Depends on `Domain`.
3.  **Infrastructure**: External implementations (DB, Email). Depends on `Application` (Interfaces) and `Domain`.
4.  **Api**: Entry point. Depends on `Application` and `Infrastructure`.

## Key Features
- **CQRS**: `CreateOrderCommand`, `GetOrderQuery`.
- **Pipeline Behaviors**: Global Logging and Validation (`FluentValidation`) pipeline.
- **Domain Modeling**: Rich Domain Model with `ValueObjects` and `DomainEvents`.
- **API Versioning**: Minimal APIs with `Asp.Versioning`.

## How to Run
```bash
dotnet run --project src/CleanArchitecture/CleanArchitecture.Api/CleanArchitecture.Api.csproj
```

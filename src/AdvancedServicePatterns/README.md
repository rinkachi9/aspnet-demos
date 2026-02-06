# AdvancedServicePatterns

Demostrates advanced Service Collection patterns and background processing in ASP.NET Core.

## Key Concepts

1.  **Scrutor & Decorators**: Automatic assembly scanning using `Scan` and implementation of the **Decorator Pattern** via `Decorate<TInterface, TDecorator>` to wrap services without changing consumer code.
2.  **Keyed Services**: Using `.NET 8` Keyed services (`AddKeyedSingleton`) to register multiple implementations of the same interface.
3.  **Background Processing with Scope**: Correctly consuming Scoped services inside a Singleton `BackgroundService` using `IServiceScopeFactory`.
4.  **Channel<T> Queue**: Implementing a high-performance in-memory queue using `System.Threading.Channels`.
5.  **Quartz.NET**: Scheduling periodic jobs (`CleanupJob`) with robust configuration.

## How to Run

```bash
dotnet run
```

### Verification
- **Background Queue**:
    - POST `/api/reports/queue?name=AnnualReport`
    - Observe logs: `Processing report: AnnualReport`.
- **Decorator**:
    - GET `/api/di/decorator`
    - Observe logs: `Before generation (Decorator)` -> `Generating report (Core Logic)` -> `After generation (Decorator)`.
- **Quartz**:
    - Wait 5 seconds and observe `Cleanup Job executing` logs.

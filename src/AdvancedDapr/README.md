# Advanced Dapr Project

Demonstrates the **Distributed Application Runtime (Dapr)** integration with ASP.NET Core 9.

## Features Implemented

### 1. Pub/Sub
- **Pattern**: Publish/Subscribe messaging.
- **Component**: Redis (`pubsub` name).
- **Code**: `DaprController.OrderSubscriber` subscribes to topic `orders`.
- **Test**: `POST /dapr/publish` or publish event via Dapr CLI.

### 2. State Management
- **Pattern**: Key/Value Store.
- **Component**: Redis (`statestore` name).
- **Code**: `DaprController` saves/gets state.
- **Feature**: Uses `state.redis`.

### 3. Service Invocation
- **Pattern**: Service-to-Service HTTP calls.
- **Code**: `DaprController.InvokeSelf` calls its own sidecar to invoke another method.
- **Benefit**: Automatic mTLS, Retries, Tracing.

### 4. Bindings
- **Pattern**: Event Triggers and Output bindings.
- **Input**: **Cron** (`binding-cron.yaml`) calls `POST /cron` every minute.
- **Output**: **Local Storage** (`binding-file.yaml`) writes output file.
- **Code**: `BindingsController`.

### 5. Actors
- **Pattern**: Virtual Actors.
- **Code**: `SmartDeviceActor`.
- **Features**: State, Timers, Reminders.

### 6. Secrets Management
- **Pattern**: Secure configuration storage.
- **Component**: Local File (`secrets` name).
- **Code**: `ExpandedController.GetSecret`.

### 7. Distributed Lock
- **Pattern**: Mutex across services.
- **Code**: `ExpandedController.AcquireLock` uses Redis to lock a resource.

### 8. Workflow (New!)
- **Pattern**: Durable workflow execution.
- **Code**: `OrderWorkflow` (Verify -> Approve).
- **Engine**: Dapr Workflow Engine (requires `dapr init`).
- **Trigger**: `POST /workflow/start`.

## How to Run

### Prerequisites
- [Dapr CLI](https://docs.dapr.io/getting-started/) installed.
- Docker Desktop running.
- `dapr init` executed.

### Run Command
Use the Dapr CLI to run the application along with its sidecar and components:

```bash
dapr run \
  --app-id advanced-dapr \
  --app-port 5000 \
  --dapr-http-port 3500 \
  --resources-path ./src/AdvancedDapr/Components \
  -- dotnet run --project src/AdvancedDapr/AdvancedDapr.csproj
```

### Testing Endpoints

**Secrets**:
```bash
curl http://localhost:5000/expanded/secrets/db-password
```

**Workflow**:
```bash
curl -X POST http://localhost:5000/workflow/start \
   -H "Content-Type: application/json" \
   -d '{"orderId": "123", "item": "Laptop", "quantity": 1}'
```

**Pub/Sub**:
```bash
dapr publish --publish-app-id advanced-dapr --pubsub pubsub --topic orders --data '{"id": "1", "content": "hello"}'
```

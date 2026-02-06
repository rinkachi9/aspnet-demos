# MessagingPatterns

Demonstrates **Asynchronous Messaging** using **MassTransit** and **RabbitMQ**.

## Key Concepts
1.  **MassTransit**: Abstraction over message broker.
2.  **Pub/Sub**: `IPublishEndpoint.Publish` sends messages to exchanges.
3.  **Consumers**: `SubmitOrderConsumer` processes messages asynchronously.
4.  **Sagas (State Machines)**: `OrderStateMachine` orchestrates the lifecycle of an order (Submitted -> Accepted).

## Infrastructure
RabbitMQ required (Management UI at http://localhost:15672).
```bash
docker compose -f docker/docker-compose.yml up -d
```

## How to Test
**Note**: This project requires RabbitMQ to be running.
1.  **Swagger**: `http://localhost:5006/swagger`
2.  **Submit Order**: `POST /orders`
    - Publishes `SubmitOrder` message.
    - `SubmitOrderConsumer` consumes it -> publishes `OrderAccepted`.
    - `OrderStateMachine` transitions state from `Submitted` to `Accepted`.

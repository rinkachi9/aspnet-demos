namespace AdvancedMassTransit.Contracts;

// COMMANDS
public record SubmitOrder(Guid OrderId, string CustomerNumber, decimal TotalAmount);
public record ValidateOrder(Guid OrderId);
public record PayOrder(Guid OrderId, decimal Amount);
public record ShipOrder(Guid OrderId);

// EVENTS
public record OrderSubmitted(Guid OrderId, DateTime Timestamp);
public record OrderValidated(Guid OrderId, bool IsValid);
public record OrderPaid(Guid OrderId);
public record OrderCompleted(Guid OrderId);
public record OrderFailed(Guid OrderId, string Reason);

// REQUEST/RESPONSE
public record CheckOrderStatus(Guid OrderId);
public record OrderStatusResult(Guid OrderId, string Status, string State);

// KAFKA TELEMETRY
public record OrderTelemetry(Guid OrderId, string Action, double LatencyMs);

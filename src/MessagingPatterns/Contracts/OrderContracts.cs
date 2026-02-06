namespace MessagingPatterns.Contracts;

public record SubmitOrder(Guid OrderId, string CustomerNumber);
public record OrderAccepted(Guid OrderId, DateTime Timestamp);
public record CheckOrderStatus(Guid OrderId);
public record OrderStatusResult(Guid OrderId, string Status, DateTime? Timestamp);

namespace AdvancedMessaging.Contracts;

public record SubmitOrder(Guid OrderId, string CustomerNumber);
public record OrderAccepted(Guid OrderId, DateTime Timestamp);
public record OrderValidated(Guid OrderId);
public record OrderFaulted(Guid OrderId, string Reason);

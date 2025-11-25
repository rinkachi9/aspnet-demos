namespace Demo.Common.Messaging.Contracts;

public record SampleMessage(Guid Id, DateTimeOffset CreatedAt, string Payload);

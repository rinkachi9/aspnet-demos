namespace EventSourcing.Api.Events;

// Events need to be simple records/classes
public record BankAccountOpened(Guid AccountId, string OwnerName, decimal InitialBalance);
public record MoneyDeposited(Guid AccountId, decimal Amount);
public record MoneyWithdrawn(Guid AccountId, decimal Amount);

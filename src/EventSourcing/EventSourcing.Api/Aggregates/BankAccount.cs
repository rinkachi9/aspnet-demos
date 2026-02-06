using EventSourcing.Api.Events;

namespace EventSourcing.Api.Aggregates;

public class BankAccount
{
    // ID is mandatory for Marten
    public Guid Id { get; set; }
    public decimal Balance { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;

    // Apply method convention - Marten automatically calls these when replaying events
    public void Apply(BankAccountOpened @event)
    {
        Id = @event.AccountId;
        OwnerName = @event.OwnerName;
        Balance = @event.InitialBalance;
    }

    public void Apply(MoneyDeposited @event)
    {
        Balance += @event.Amount;
    }

    public void Apply(MoneyWithdrawn @event)
    {
        Balance -= @event.Amount;
    }
}

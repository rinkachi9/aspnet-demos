using EventSourcing.Api.Events;
using Marten.Events.Aggregation;

namespace EventSourcing.Api.Projections;

// This projection will create a queryable "View" of the account
public class BankAccountDetails
{
    public Guid Id { get; set; }
    public decimal CurrentBalance { get; set; }
    public string AccountOwner { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
}

public class BankAccountDetailsProjection : SingleStreamProjection<BankAccountDetails>
{
    public BankAccountDetailsProjection()
    {
        // Define how events mutate the Read Model
        ProjectEvent<BankAccountOpened>((view, @event) =>
        {
            view.Id = @event.AccountId;
            view.AccountOwner = @event.OwnerName;
            view.CurrentBalance = @event.InitialBalance;
            view.TransactionCount = 1; // Opening is a transaction
        });

        ProjectEvent<MoneyDeposited>((view, @event) =>
        {
            view.CurrentBalance += @event.Amount;
            view.TransactionCount++;
        });

        ProjectEvent<MoneyWithdrawn>((view, @event) =>
        {
            view.CurrentBalance -= @event.Amount;
            view.TransactionCount++;
        });
    }
}

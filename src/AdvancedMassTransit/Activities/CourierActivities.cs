using MassTransit;

namespace AdvancedMassTransit.Activities;

// ARGUMENTS
public record BlockFundsArguments(Guid OrderId, decimal Amount, string AccountNumber);
public record BlockFundsLog(Guid TransactionId, decimal Amount); // For Compensation

public class BlockFundsActivity : IActivity<BlockFundsArguments, BlockFundsLog>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<BlockFundsArguments> context)
    {
        Console.WriteLine($"[Activity] Blocking ${context.Arguments.Amount} for Account {context.Arguments.AccountNumber}");
        
        // Return Log for Compensation
        return context.Completed(new BlockFundsLog(Guid.NewGuid(), context.Arguments.Amount));
    }

    public async Task<CompensationResult> Compensate(CompensateContext<BlockFundsLog> context)
    {
        Console.WriteLine($"[Compensate] Unblocking ${context.Log.Amount} (Tx: {context.Log.TransactionId})");
        return context.Compensated();
    }
}

// ARGUMENTS
public record AllocateInventoryArguments(Guid OrderId, string Sku, int Quantity);
public record AllocateInventoryLog(Guid AllocationId);

public class AllocateInventoryActivity : IActivity<AllocateInventoryArguments, AllocateInventoryLog>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
    {
        Console.WriteLine($"[Activity] Allocating {context.Arguments.Quantity} of {context.Arguments.Sku}");

        if (context.Arguments.Quantity > 100)
            throw new Exception("Not enough inventory!"); // Triggers Compensation of previous steps

        return context.Completed(new AllocateInventoryLog(Guid.NewGuid()));
    }

    public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLog> context)
    {
        Console.WriteLine($"[Compensate] Releasing Inventory (Alloc: {context.Log.AllocationId})");
        return context.Compensated();
    }
}

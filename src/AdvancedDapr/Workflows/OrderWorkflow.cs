using Dapr.Workflow;

namespace AdvancedDapr.Workflows;

public record OrderInfo(string OrderId, string Item, int Quantity);
public record WorkflowResult(string Status, string Details);

public class OrderWorkflow : Workflow<OrderInfo, WorkflowResult>
{
    public override async Task<WorkflowResult> RunAsync(WorkflowContext context, OrderInfo input)
    {
        var orderId = input.OrderId;

        // 1. Verify
        var verificationResult = await context.CallActivityAsync<bool>(
            nameof(VerifyOrderActivity), 
            input);

        if (!verificationResult)
        {
            return new WorkflowResult("Rejected", "Order validation failed");
        }

        // 2. Approve
        await context.CallActivityAsync(
            nameof(ApproveOrderActivity), 
            orderId);

        return new WorkflowResult("Completed", $"Order {orderId} processed successfully");
    }
}

public class VerifyOrderActivity : WorkflowActivity<OrderInfo, bool>
{
    public override Task<bool> RunAsync(WorkflowActivityContext context, OrderInfo input)
    {
        // Simulate check
        var valid = input.Quantity > 0 && !string.IsNullOrEmpty(input.Item);
        return Task.FromResult(valid);
    }
}

public class ApproveOrderActivity : WorkflowActivity<string, object?>
{
    private readonly ILogger _logger;

    public ApproveOrderActivity(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ApproveOrderActivity>();
    }

    public override Task<object?> RunAsync(WorkflowActivityContext context, string orderId)
    {
        _logger.LogInformation("Approving Order: {OrderId}", orderId);
        return Task.FromResult<object?>(null);
    }
}

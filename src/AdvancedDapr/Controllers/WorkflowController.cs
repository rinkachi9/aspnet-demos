using AdvancedDapr.Workflows;
using Dapr.Client;
using Dapr.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedDapr.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly DaprWorkflowClient _workflowClient;

    public WorkflowController(DaprWorkflowClient workflowClient)
    {
        _workflowClient = workflowClient;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartWorkflow([FromBody] OrderInfo order)
    {
        var instanceId = Guid.NewGuid().ToString();
        await _workflowClient.ScheduleNewWorkflowAsync(
            nameof(OrderWorkflow), 
            instanceId, 
            order);

        return Accepted(new { instanceId });
    }

    [HttpGet("{instanceId}")]
    public async Task<IActionResult> GetStatus(string instanceId)
    {
        var state = await _workflowClient.GetWorkflowStateAsync(instanceId, true);
        return Ok(state);
    }
}

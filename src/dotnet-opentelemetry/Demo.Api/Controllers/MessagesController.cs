using Demo.Api.Contracts;
using Demo.Api.Services;
using Demo.Common.Telemetry;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessagePublisher _publisher;

    public MessagesController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Publish([FromBody] PublishMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await _publisher.PublishAsync(request.Payload, cancellationToken);
        return Accepted($"/api/messages/{message.Id}", new { id = message.Id, service = Telemetry.ServiceNameApi });
    }

    // Legacy alias to keep the original sample curl working.
    [HttpPost("/publish")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public Task<IActionResult> PublishLegacy([FromBody] string payload, CancellationToken cancellationToken) =>
        Publish(new PublishMessageRequest { Payload = payload }, cancellationToken);
}

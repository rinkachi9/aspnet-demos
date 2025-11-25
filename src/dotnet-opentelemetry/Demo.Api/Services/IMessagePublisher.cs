using Demo.Common.Messaging.Contracts;

namespace Demo.Api.Services;

public interface IMessagePublisher
{
    Task<SampleMessage> PublishAsync(string payload, CancellationToken cancellationToken);
}

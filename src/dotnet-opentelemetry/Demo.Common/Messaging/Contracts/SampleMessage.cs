using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Demo.Common.Messaging.Contracts;

public record SampleMessage(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Payload
)
{
    [JsonIgnore]
    public ActivityContext? ParentContext { get; init; }
}



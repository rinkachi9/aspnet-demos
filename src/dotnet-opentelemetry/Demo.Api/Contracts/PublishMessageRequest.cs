using System.ComponentModel.DataAnnotations;

namespace Demo.Api.Contracts;

public class PublishMessageRequest
{
    [Required]
    public string Payload { get; init; } = string.Empty;
}

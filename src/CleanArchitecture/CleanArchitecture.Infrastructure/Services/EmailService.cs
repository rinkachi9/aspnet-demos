using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderConfirmationAsync(Order order)
    {
        _logger.LogInformation("Sending confirmation email for Order #{OrderId} to Customer #{CustomerId}.", order.Id, order.CustomerId);
        return Task.CompletedTask;
    }
}

namespace AdvancedServicePatterns.Services;

public interface INotificationService
{
    Task NotifyAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyAsync(string message)
    {
        // Simulate slow operation
        await Task.Delay(2000); 
        _logger.LogInformation("[Email] Sending email notification: {Message}", message);
    }
}

public class SmsNotificationService : INotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(ILogger<SmsNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(string message)
    {
        // Fast operation
        _logger.LogInformation("[SMS] Sending SMS notification: {Message}", message);
        return Task.CompletedTask;
    }
}

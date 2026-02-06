namespace BackgroundJobsHangfire.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendWelcomeEmail(string email)
    {
        _logger.LogInformation("Sending welcome email to {Email}...", email);
        await Task.Delay(1000); // Simulate work
        _logger.LogInformation("Email sent to {Email}!", email);
    }

    public void ProcessMonthlyReport()
    {
        _logger.LogInformation("Starting Monthly Report Processing...");
        // Simulate long running CPU work
        Thread.Sleep(500); 
        _logger.LogInformation("Monthly Report Done.");
    }
}

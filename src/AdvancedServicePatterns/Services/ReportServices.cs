namespace AdvancedServicePatterns.Services;

public interface IReportService
{
    void Generate(string reportName);
}

public class DefaultReportService : IReportService
{
    private readonly ILogger<DefaultReportService> _logger;

    public DefaultReportService(ILogger<DefaultReportService> logger)
    {
        _logger = logger;
    }

    public void Generate(string reportName)
    {
        _logger.LogInformation("Generating report (Core Logic): {ReportName}", reportName);
    }
}

// Decorator pattern using Scrutor will wrap the implementation
public class LoggingReportDecorator : IReportService
{
    private readonly IReportService _inner;
    private readonly ILogger<LoggingReportDecorator> _logger;

    public LoggingReportDecorator(IReportService inner, ILogger<LoggingReportDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Generate(string reportName)
    {
        _logger.LogInformation("Before generation (Decorator)");
        _inner.Generate(reportName);
        _logger.LogInformation("After generation (Decorator)");
    }
}

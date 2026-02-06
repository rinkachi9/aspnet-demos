namespace AdvancedServicePatterns.Models;

public record ReportRequest(Guid Id, string ReportName, string NotifyType, DateTime CreatedAt);

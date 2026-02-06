namespace BackgroundJobsQuartz.Options;

public class ReportJobOptions
{
    public const string SectionName = "Quartz:Jobs:ReportGenerator";

    public int RetryCount { get; set; } = 3;
    public string ReportName { get; set; } = "Default Report";
}

namespace BackgroundJobsQuartz.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class QuartzJobAttribute : Attribute
{
    public string CronExpression { get; }
    public string? Identity { get; set; }
    public string? Group { get; set; }

    public QuartzJobAttribute(string cronExpression)
    {
        CronExpression = cronExpression;
    }
}

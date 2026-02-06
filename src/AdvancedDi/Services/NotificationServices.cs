namespace AdvancedDi.Services;

public interface INotificationService
{
    string Notify(string message);
}

public class EmailService : INotificationService
{
    public string Notify(string message) => $"[Email] Sent: {message}";
}

public class SmsService : INotificationService
{
    public string Notify(string message) => $"[SMS] Sent: {message}";
}

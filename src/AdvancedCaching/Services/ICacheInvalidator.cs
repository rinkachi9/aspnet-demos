namespace AdvancedCaching.Services;

public interface ICacheInvalidator
{
    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
}

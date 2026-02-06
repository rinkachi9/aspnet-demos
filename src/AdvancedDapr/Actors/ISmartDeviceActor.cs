using Dapr.Actors;
using Dapr.Actors.Runtime;

namespace AdvancedDapr.Actors;

public interface ISmartDeviceActor : IActor
{
    Task SetDataAsync(DeviceData data);
    Task<DeviceData> GetDataAsync();
    Task RegisterReminder();
    Task UnregisterReminder();
}

public record DeviceData(double Temperature, double Humidity, DateTime Timestamp);

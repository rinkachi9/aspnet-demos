using Dapr.Actors.Runtime;

namespace AdvancedDapr.Actors;

public class SmartDeviceActor : Actor, ISmartDeviceActor, IRemindable
{
    private const string STATE_KEY = "device_data";

    public SmartDeviceActor(ActorHost host) : base(host)
    {
    }

    public async Task SetDataAsync(DeviceData data)
    {
        await StateManager.SetStateAsync(STATE_KEY, data);
    }

    public async Task<DeviceData> GetDataAsync()
    {
        return await StateManager.GetStateAsync<DeviceData>(STATE_KEY);
    }

    public async Task RegisterReminder()
    {
        await RegisterReminderAsync("SensorCheck", null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
    }

    public async Task UnregisterReminder()
    {
        await UnregisterReminderAsync("SensorCheck");
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName == "SensorCheck")
        {
            // Logic to check sensor, e.g. log
            // Console.WriteLine($"[Actor {Id}] Reminder triggered.");
            Logger.LogInformation("[Actor {Id}] Reminder triggered.", Id);
        }
        return Task.CompletedTask;
    }
}

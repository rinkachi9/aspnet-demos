using System.Text.Json;
using KafkaFlow;

namespace Demo.Common.Kafka;

public class JsonMessageDeserializer : IDeserializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public object Deserialize(byte[] data, Type type)
    {
        if (data is null or { Length: 0 })
            return Activator.CreateInstance(type)!;

        return JsonSerializer.Deserialize(data, type, Options)!;
    }

    public async Task<object> DeserializeAsync(Stream? input, Type type, ISerializerContext context)
    {
        if (input == null || input.Length == 0)
        {
            return Activator.CreateInstance(type)!;
        }

        var result = await JsonSerializer.DeserializeAsync(input, type, Options);

        return result ?? Activator.CreateInstance(type)!;
    }
}

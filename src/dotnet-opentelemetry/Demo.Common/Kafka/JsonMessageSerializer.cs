using System.Text;
using System.Text.Json;
using KafkaFlow;

namespace Demo.Common.Kafka;

public class JsonMessageSerializer : ISerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public byte[] Serialize(object obj)
    {
        if (obj is null)
            return Array.Empty<byte>();

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, obj.GetType(), Options));
    }

    public async Task SerializeAsync(object message, Stream output, ISerializerContext context)
    {
        if (message is null)
            return;

        await JsonSerializer.SerializeAsync(output, message, message.GetType(), Options);
    }
}

using Grpc.Core;
using Grpc.Net.Client;
using GrpcService.Api; // Namespace from Proto

// Allow gRPC over HTTP (unencrypted)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var channel = GrpcChannel.ForAddress("http://localhost:5130");
var client = new Weather.WeatherClient(channel);

Console.WriteLine("--- Unary Call ---");
var reply = await client.GetWeatherAsync(new WeatherRequest { City = "Warsaw" });
Console.WriteLine($"Got: {reply.City}, Temp: {reply.TemperatureC}C, Desc: {reply.Description}");

Console.WriteLine("\n--- Server Streaming ---");
using var call = client.MonitorWeather(new WeatherRequest { City = "Krakow" });

await foreach (var response in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"Stream Update: {response.City} - {response.TemperatureC}C ({response.Timestamp})");
}

Console.WriteLine("Done.");

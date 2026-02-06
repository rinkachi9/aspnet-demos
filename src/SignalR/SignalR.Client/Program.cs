using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5075/chat")
    .WithAutomaticReconnect()
    .Build();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"[{user}]: {message}");
});

try 
{
    await connection.StartAsync();
    Console.WriteLine("Connected to ChatHub!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return;
}

// Send a message
await connection.InvokeAsync("SendMessage", "ConsoleClient", "Hello from C# Client!");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

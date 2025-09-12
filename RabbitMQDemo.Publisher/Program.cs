using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Text.Json;
using System;

// 1. Set RabbitMQ host
string rabbitHost = "10.123.123.31"; // your server LAN IP or localhost if same PC

using var rabbit = new RabbitMQService(rabbitHost, "guest", "guest");

Console.WriteLine("Publisher started.");

// 1️⃣ Generate a unique client ID for this PC
string localClientId = $"client.{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0, 5)}";
Console.WriteLine($"This PC's unique Client ID: {localClientId}");

// Loop to send messages
while (true)
{
    // 2️⃣ Optional: ask for target client ID or default to broadcast
    Console.Write("Enter Target Client ID (or leave empty for broadcast): ");
    string targetClientId = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(targetClientId))
    {
        targetClientId = "all"; // or some default queue
    }

    Console.Write("Enter Message: ");
    string messageText = Console.ReadLine();

    // Prepare request
    var request = new ClientRequest
    {
        ClientId = localClientId, // sender's unique ID
        Method = "Print",
        Payload = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Message", messageText }
        }
    };

    // Ensure queue exists & publish
    string queueName = $"client.{targetClientId}";
    rabbit.QueueDeclareAndBind(queueName);
    rabbit.Publish(queueName, request);

    Console.WriteLine($"Message sent to {targetClientId}.\n");
}
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Text.Json;
using System;
using System.Collections.Generic;

// 1. Set RabbitMQ host
string rabbitHost = "10.123.123.31"; // replace with your server IP
string rabbitUser = "guest";
string rabbitPass = "guest";

// 2. Set server token (same as in appsettings.json of server)
string serverToken = "my_secure_token";

using var rabbit = new RabbitMQService(rabbitHost, rabbitUser, rabbitPass);

Console.WriteLine("Publisher started.");

// 3. Generate a unique client ID for this PC
string localClientId = $"client.{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0, 5)}";
Console.WriteLine($"This PC's unique Client ID: {localClientId}");

while (true)
{
    Console.Write("Enter Target Client ID (leave empty for broadcast): ");
    string targetClientId = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(targetClientId))
    {
        targetClientId = "all"; // default queue for broadcast
    }

    // Ask for KOT details
    Console.Write("Bill No: ");
    string billNo = Console.ReadLine() ?? "";

    Console.Write("Table No: ");
    string tableNo = Console.ReadLine() ?? "";

    Console.Write("Waiter Name: ");
    string waiter = Console.ReadLine() ?? "";

    Console.Write("Items (format: Item1,Qty1;Item2,Qty2): ");
    string items = Console.ReadLine() ?? "";

    // Prepare ClientRequest
    var request = new ClientRequest
    {
        ClientId = localClientId,
        Method = "Print",      // for KOT printing
        Token = serverToken,   // token for client validation
        IsPing = false,
        Payload = new Dictionary<string, string>
        {
            { "BillNo", billNo },
            { "TableNo", tableNo },
            { "Waiter", waiter },
            { "Items", items }
        }
    };

    string queueName = $"client.{targetClientId}";
    rabbit.QueueDeclareAndBind(queueName);
    rabbit.Publish(queueName, request);

    Console.WriteLine($"✅ KOT sent to {targetClientId} with token.\n");
}

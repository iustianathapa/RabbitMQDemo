using Microsoft.Extensions.Configuration;
using RabbitMQDemo.Shared;
using System;
using System.Threading.Tasks;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Get RabbitMQ settings
var rabbitHost = configuration["RabbitMQ:Host"];
var rabbitUser = configuration["RabbitMQ:User"];
var rabbitPass = configuration["RabbitMQ:Password"];

// Get client ID from user input
Console.Write("Enter Client ID (same as publisher will use, e.g., client1): ");
string clientId = Console.ReadLine()?.Trim() ?? "client1";

using var rabbit = new RabbitMQService(rabbitHost, rabbitUser, rabbitPass);

Console.WriteLine($"Client {clientId} started. Waiting for messages...");
Console.WriteLine($"Publisher will send messages to queue: client.{clientId}");

// Start consuming messages for this client
rabbit.Consume($"client.{clientId}", async (message) =>
{
    Console.WriteLine($"Received: {message}");
    await Task.CompletedTask;
});

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
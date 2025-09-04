using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Text.Json;

// 1. Set RabbitMQ host
string rabbitHost = "10.123.125.22"; // your server LAN IP or localhost if same PC

using var rabbit = new RabbitMQService(rabbitHost);

Console.WriteLine("Publisher started.");

// Loop to send messages
while (true)
{
    Console.Write("Enter Client ID (e.g., client.1): ");
    string clientId = Console.ReadLine();

    Console.Write("Enter Message: ");
    string messageText = Console.ReadLine();

    // Prepare request
    var request = new ClientRequest
    {
        ClientId = clientId,
        Method = "Print",
        Payload = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Message", messageText }
        }
    };

    // Ensure queue exists & publish
    rabbit.QueueDeclareAndBind($"client.{clientId}");
    rabbit.Publish($"client.{clientId}", request);

    Console.WriteLine($"Message sent to {clientId}.\n");
}
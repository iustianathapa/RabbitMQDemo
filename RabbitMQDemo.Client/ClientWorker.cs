using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using RabbitMQDemo.Client.Models;
using RabbitMQDemo.Client.Services;
using System.Text.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQDemo.Client
{
    public class ClientWorker : BackgroundService
    {
        private readonly RabbitMQService _rabbit;
        private readonly string _clientId;
        private readonly string _expectedToken;
        private readonly PrintManager _printManager;

        public ClientWorker(RabbitMQService rabbit, IConfiguration config, string printerName = "DefaultPrinter")
        {
            _rabbit = rabbit;
            _clientId = config["Client:ClientId"] ?? "unknown";
            _expectedToken = config["Client:Token"] ?? "";
            _printManager = new PrintManager(printerName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = $"client.{_clientId}";

            Console.WriteLine($"[{_clientId}] Connected to queue: {queueName}");
            Console.WriteLine($"[{_clientId}] Waiting for messages...");

            _rabbit.Consume(queueName, async message =>
            {
                try
                {
                    // First try to parse as generic ClientRequest
                    var request = JsonSerializer.Deserialize<ClientRequest>(message);

                    if (request == null)
                    {
                        Console.WriteLine($"[{_clientId}] Invalid message format.");
                        return;
                    }

                    // Token validation
                    if (request.Token != _expectedToken)
                    {
                        Console.WriteLine($"[{_clientId}] Invalid token. Ignoring message.");
                        return;
                    }

                    // ✅ Handle Ping
                    if (request.IsPing)
                    {
                        Console.WriteLine($"[{_clientId}] 🔔 Ping received from server.");
                        // Optionally reply with Pong via RabbitMQ here
                        return;
                    }

                    // ✅ Handle normal KOT payload
                    if (request.Method == "Print" || request.Method == "KOT")
                    {
                        var kotPayload = JsonSerializer.Deserialize<KotBotPayload>(message);

                        if (kotPayload?.payload?.Master == null)
                        {
                            Console.WriteLine($"[{_clientId}] Missing KOT payload data.");
                            return;
                        }

                        Console.WriteLine($"[{_clientId}] Processing KOT for bill: {kotPayload.payload.Master.BillNo}");

                        var result = _printManager.PrintBill(kotPayload.payload);
                        Console.WriteLine($"[{_clientId}] Print result: {result.Status} - {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{_clientId}] Error processing message: {ex.Message}");
                }

                await Task.CompletedTask;
            });

            return Task.CompletedTask;
        }
    }
}

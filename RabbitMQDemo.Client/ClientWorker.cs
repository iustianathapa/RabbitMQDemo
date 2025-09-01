using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace RabbitMQDemo.Client
{
    public class ClientWorker : BackgroundService
    {
        private readonly RabbitMQService _rabbit;
        private readonly string _clientId;

        public ClientWorker(RabbitMQService rabbit, string clientId)
        {
            _rabbit = rabbit;
            _clientId = clientId;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbit.Consume($"client.{_clientId}", async message =>
            {
                var request = JsonSerializer.Deserialize<ClientRequest>(message);
                Console.WriteLine($"[{_clientId}] Received method: {request.Method}");

                if (request.Method == "Print" && request.Payload.ContainsKey("Message"))
                {
                    Console.WriteLine($"[{_clientId}] Message: {request.Payload["Message"]}");
                }

                await Task.CompletedTask;
            });

            return Task.CompletedTask;
        }
    }
}
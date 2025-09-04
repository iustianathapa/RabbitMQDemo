using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
            string queueName = $"client.{_clientId}";

            _rabbit.Consume(queueName, async message =>
            {
                var request = JsonSerializer.Deserialize<ClientRequest>(message);
                Console.WriteLine($"[{_clientId}] Received: {request.Method} - {request.Payload["Message"]}");
                await Task.CompletedTask;
            });

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _rabbit.Dispose();
            base.Dispose();
        }
    }
}
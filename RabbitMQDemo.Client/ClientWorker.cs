using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client.Models;
using RabbitMQDemo.Client.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQDemo.Client
{
    public class ClientWorker : BackgroundService
    {
        private readonly RabbitMQService _rabbit;
        private readonly string _clientId;
        private readonly PrintManager _printManager;

        public ClientWorker(RabbitMQService rabbit, string clientId, string printerName = "DefaultPrinter")
        {
            _rabbit = rabbit;
            _clientId = clientId;
            _printManager = new PrintManager(printerName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string queueName = $"{_clientId}";

            Console.WriteLine($"[{_clientId}] Connected to queue: {queueName}");
            Console.WriteLine($"[{_clientId}] Waiting for messages...");

            _rabbit.Consume(queueName, async message =>
            {
                try
                {
                    var request = JsonSerializer.Deserialize<ClientRequest>(message);

                    if (request != null)
                    {
                        Console.WriteLine($"[{_clientId}] Received Method: {request.Method}");
                        
                        if (request.Method.Equals("Print", StringComparison.OrdinalIgnoreCase))
                        {
                            PrintKOT(request.Payload);
                        }
						if (request?.Method == "Ping" && !string.IsNullOrEmpty(request.ReplyTo))
						{
							var response = new ClientPingResponse
							{
								CorrelationId = request.CorrelationId,
								Status = "Ok"
							};
							_rabbit.Publish(request.ReplyTo ?? "", response);
						}
					}

                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{_clientId}] Error processing message: {ex.Message}");
                }
            });

            return Task.CompletedTask;
        }


        private void PrintKOT(PrintVM payload)
        {
            try
            {
               _printManager.PrintBill(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_clientId}] Failed to print KOT: {ex.Message}");
            }
        }
        public override void Dispose()
        {
            _rabbit.Dispose();
            base.Dispose();
        }
    }
	
}

using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
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
            string queueName = $"client.{_clientId}";

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
                        foreach (var kvp in request.Payload)
                        {
                            Console.WriteLine($"[{_clientId}] {kvp.Key}: {kvp.Value}");
                        }

                        if (request.Method.Equals("Print", StringComparison.OrdinalIgnoreCase))
                        {
                            PrintKOT(request.Payload);
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

        private void PrintKOT(Dictionary<string, string> payload)
        {
            try
            {
                var printData = new PrintVM
                {
                    Master = new MasterVM
                    {
                        BillNo = payload.GetValueOrDefault("BillNo", ""),
                        TableNo = payload.GetValueOrDefault("TableNo", ""),
                        Waiter = payload.GetValueOrDefault("Waiter", ""),
                        SerialNo = payload.GetValueOrDefault("BillNo", "")
                    },
                    Details = ParseItems(payload.GetValueOrDefault("Items", ""))
                };

                _printManager.PrintBill(printData);
                Console.WriteLine($"[{_clientId}] KOT printed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_clientId}] Failed to print KOT: {ex.Message}");
            }
        }

        private List<DetailVM> ParseItems(string itemsString)
        {
            var details = new List<DetailVM>();
            if (!string.IsNullOrEmpty(itemsString))
            {
                var items = itemsString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    var parts = item.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        details.Add(new DetailVM
                        {
                            ItemName = parts[0].Trim(),
                            Qty = parts[1].Trim()
                        });
                    }
                }
            }
            return details;
        }

        public override void Dispose()
        {
            _rabbit.Dispose();
            base.Dispose();
        }
    }
}

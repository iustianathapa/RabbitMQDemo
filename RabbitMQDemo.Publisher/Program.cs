using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Text.Json;
using System;

// 1. Set RabbitMQ host
string rabbitHost = "localhost"; // your server LAN IP or localhost if same PC

using var rabbit = new RabbitMQService(rabbitHost, "guest", "guest");

Console.WriteLine("Publisher started.");

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
	// Prepare request — create a real PrintVM instance
	var request = new ClientRequest
	{
		ClientId = targetClientId, // sender's unique ID
		Method = "Print",
		Payload = new PrintVM
		{
			IsCancellationBill = false,
			Master = new PrintMasteVM
			{
				BillNo = "BILL-0001",
				Date = DateTime.Now.ToString("yyyy-MM-dd"),
				TableNo = "T1",
				Waiter = Environment.UserName,
				Time = DateTime.Now.ToString("HH:mm"),
				Type = "KOT",
				SerialNo = Guid.NewGuid().ToString(),
				PrinterName = "DefaultPrinter",
				PrintedBy = targetClientId,
				NoOfKotBotToBePrinted = 1
			},
			Details = new List<PrintDetailVM>
			{
				new PrintDetailVM
				{
					ItemName = messageText, // place your message here
                    Quantity = 1,
					Remarks = string.Empty
				}
			}
		}
	};

	// Ensure queue exists & publish
	string queueName = $"{targetClientId}";
    rabbit.QueueDeclareAndBind(queueName);
    rabbit.Publish(queueName, request);

    Console.WriteLine($"Message sent to {targetClientId}.\n");
}
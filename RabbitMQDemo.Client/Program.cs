using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client;
using System;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // 1️⃣ Generate a unique client ID dynamically if not in config
        string clientId = config["ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            clientId = $"client.{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0,5)}";
        }

        // 2️⃣ RabbitMQ connection details
        string host = config["RabbitMQ:Host"];
        if (string.IsNullOrEmpty(host))
            throw new Exception("RabbitMQ:Host not found in configuration!");

        string user = config["RabbitMQ:User"] ?? "guest";
        string pass = config["RabbitMQ:Password"] ?? "guest";

        // 3️⃣ Use your existing RabbitMQService constructor
        var rabbitService = new RabbitMQService(host, user, pass);

        services.AddSingleton(rabbitService);

        // 4️⃣ Register the hosted client worker
        services.AddHostedService(sp => new ClientWorker(
            sp.GetRequiredService<RabbitMQService>(), clientId));

        Console.WriteLine($"Client ID for this PC: {clientId}");
    })
    .Build();

await builder.RunAsync();
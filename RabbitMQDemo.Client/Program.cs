using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        string rabbitHost = config["RabbitMQ:Host"] ?? "localhost";
        string rabbitUser = config["RabbitMQ:User"] ?? "guest";
        string rabbitPass = config["RabbitMQ:Password"] ?? "guest";

        Console.Write("Enter Client ID: ");
        string clientId = Console.ReadLine()?.Trim() ?? "client1";

        var rabbitService = new RabbitMQService(rabbitHost, rabbitUser, rabbitPass);
        services.AddSingleton(rabbitService);

        services.AddHostedService(sp => new ClientWorker(rabbitService, clientId));
    })
    .Build();

await host.RunAsync();
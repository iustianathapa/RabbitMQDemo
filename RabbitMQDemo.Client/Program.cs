using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        string clientId = config["ClientId"] ?? "client1";

        services.AddSingleton(sp => new RabbitMQService(config["RabbitMQ:Host"]));
        services.AddHostedService(sp => new ClientWorker(sp.GetRequiredService<RabbitMQService>(), clientId));
    })
    .Build();

await builder.RunAsync();
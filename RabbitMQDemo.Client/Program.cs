using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        string clientId = config["ClientId"] ?? "client1";

        // This line is critical — make sure Host is not null
        string host = config["RabbitMQ:Host"];
        if(string.IsNullOrEmpty(host))
            throw new Exception("RabbitMQ:Host not found in configuration!");

        services.AddSingleton(sp => new RabbitMQService(host));
        services.AddHostedService(sp => new ClientWorker(
            sp.GetRequiredService<RabbitMQService>(), clientId));
    })
    .Build();

await builder.RunAsync();

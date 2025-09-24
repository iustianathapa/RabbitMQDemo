using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Client;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        services.AddSingleton(new RabbitMQService(
            config["RabbitMQ:Host"]!,
            config["RabbitMQ:User"]!,
            config["RabbitMQ:Password"]!
        ));

        services.AddHostedService<ClientWorker>();
    })
    .Build()
    .Run();
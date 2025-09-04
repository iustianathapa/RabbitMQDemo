using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQDemo.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton(sp =>
    new RabbitMQService(
        builder.Configuration["RabbitMQ:Host"],
        builder.Configuration["RabbitMQ:User"],
        builder.Configuration["RabbitMQ:Password"]
    )
);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
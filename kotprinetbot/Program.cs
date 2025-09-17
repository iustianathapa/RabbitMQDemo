using HotelBillPrinting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Register PrintManager as a singleton (or scoped if needed)
builder.Services.AddSingleton<PrintManager>(sp =>
{
    // Replace with your actual printer name
    string printerName = "YourPrinterName";
    return new PrintManager(printerName);
});

// Register RabbitMQ background service
builder.Services.AddHostedService<RabbitMqPrinterService>();

builder.Services.AddControllers();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HMS Printing API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HMS Printing API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
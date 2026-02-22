using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RatesService.Data;
using RatesService.Queries;
using RatesService.Services;
using RatesService.Workers;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, lc) => lc.MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext());

var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "CurrencyRates";

builder.Services.AddDbContext<RatesDbContext>(options =>
{
    var client = new MongoClient(mongoConnectionString);
    options.UseMongoDB(client, mongoDatabaseName);
});

var nbpBaseUrl = builder.Configuration["NbpApi:BaseUrl"] ?? "https://api.nbp.pl";
builder.Services.AddHttpClient<INbpApiClient, NbpApiClient>(client =>
{
    client.BaseAddress = new Uri(nbpBaseUrl);
});

var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:ConnectionString is required");

builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
builder.Services.AddSingleton<IRatePublisher, RatePublisher>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHostedService<RateFetchWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapGet("/api/rates/latest", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetLatestRatesQuery());

    return result is not null
        ? Results.Ok(result)
        : Results.NotFound("No exchange rate tables available");
});

app.Run();
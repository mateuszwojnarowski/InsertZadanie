using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using WalletService.Data;
using WalletService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, lc) => lc.MinimumLevel.Information()
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
.Enrich.FromLogContext());
builder.Host.UseSerilog();

// SQL Server + EF Core
builder.Services.AddDbContext<WalletDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("WalletDb"));
});

// HTTP Client for fallback
var ratesBaseUrl = builder.Configuration["RatesService:BaseUrl"] ?? "http://localhost:5100";
builder.Services.AddHttpClient("RatesService", client =>
{
    client.BaseAddress = new Uri(ratesBaseUrl);
});

// Redis
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

// Azure ServiceBus
var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"]
    ?? throw new InvalidOperationException("ServiceBus:ConnectionString is required");

builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Services
builder.Services.AddSingleton<IRateCacheService, RateCacheService>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddHostedService<RatesConsumer>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create database schema from OnModelCreating
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    Log.Information("Ensuring database is created...");
    db.Database.EnsureCreated();
    Log.Information("Database ready");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();

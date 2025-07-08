using TrafficCollector.Services.Core;
using TrafficCollector.Services.Core.Interfaces;
using TrafficCollector.Services.Health;
using TrafficCollector.Services.Background;
using TrafficCollector.Services.ErrorHandling;
using TrafficCollector.Services.Health.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TrafficCollectorOptions>(
    builder.Configuration.GetSection("TrafficCollector"));

builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();

builder.Services.AddHttpClient<ITrafficProcessor, TrafficProcessor>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(2);
    client.DefaultRequestHeaders.Add("User-Agent", "NetworkAISuite-TrafficCollector/1.0");
    client.DefaultRequestHeaders.ConnectionClose = false;
});

builder.Services.AddSingleton<ITrafficProcessor, TrafficProcessor>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddHostedService<SyslogCollectorService>();
builder.Services.AddHostedService<SnmpCollectorService>();

builder.Services.AddHealthChecks()
    .AddCheck<TrafficCollectorHealthCheck>("traffic-collector");
builder.Services.AddSingleton<IKafkaHealthService, KafkaHealthService>();

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_048_576;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

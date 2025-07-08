using TrafficCollector.Models.Events;
using TrafficCollector.Services.Core.Interfaces;

namespace TrafficCollector.Services.Core;

public class MockKafkaProducer(ILogger<MockKafkaProducer> logger) : IKafkaProducer
{
    private readonly ILogger<MockKafkaProducer> _logger = logger;

    public Task ProduceAsync(NetworkTrafficEvent trafficEvent)
    {
        _logger.LogInformation("Mock: Would produce event {Id} from {DeviceIp} - {Protocol} {SourceIp}:{SourcePort} -> {DestinationIp}:{DestinationPort}",
            trafficEvent.Id, trafficEvent.DeviceIp, trafficEvent.Protocol,
            trafficEvent.SourceIp, trafficEvent.SourcePort,
            trafficEvent.DestinationIp, trafficEvent.DestinationPort);

        return Task.CompletedTask;
    }

    public Task ProduceAsync(IEnumerable<NetworkTrafficEvent> trafficEvents)
    {
        _logger.LogInformation("Mock: Would produce batch of {Count} events", trafficEvents.Count());
        return Task.CompletedTask;
    }

    public Task<bool> IsHealthyAsync() => Task.FromResult(true);

    public void Dispose() =>
        _logger.LogInformation("Mock Kafka producer disposed");
}
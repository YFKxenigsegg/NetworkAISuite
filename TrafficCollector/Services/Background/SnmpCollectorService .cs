using TrafficCollector.Models;
using TrafficCollector.Extensions;
using Microsoft.Extensions.Options;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Collections.Concurrent;
using TrafficCollector.Services.Core.Interfaces;
using TrafficCollector.Models.Events;
using TrafficCollector.Models.Snmp;
using TrafficCollector.Constants;

namespace TrafficCollector.Services.Background;

public class SnmpCollectorService : BackgroundService
{
    private readonly TrafficCollectorOptions _options;
    private readonly ITrafficProcessor _trafficProcessor;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<SnmpCollectorService> _logger;
    private readonly ConcurrentQueue<NetworkTrafficEvent> _eventQueue = [];
    private readonly Timer _flushTimer;

    public SnmpCollectorService(
        IOptions<TrafficCollectorOptions> options,
        ITrafficProcessor trafficProcessor,
        IKafkaProducer kafkaProducer,
        ILogger<SnmpCollectorService> logger)
    {
        _options = options.Value;
        _trafficProcessor = trafficProcessor;
        _kafkaProducer = kafkaProducer;
        _logger = logger;

        _flushTimer = new Timer(FlushEvents, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_options.Processing.FlushIntervalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Snmp.Targets.Any())
        {
            _logger.LogInformation("No SNMP targets configured, SNMP collector disabled");
            return;
        }

        _logger.LogInformation("SNMP collector started with {TargetCount} targets, polling every {IntervalSeconds}s",
            _options.Snmp.Targets.Count, _options.Snmp.PollingIntervalSeconds);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PollAllTargets(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_options.Snmp.PollingIntervalSeconds), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SNMP collector stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SNMP collector");
        }
    }

    private async Task PollAllTargets(CancellationToken cancellationToken)
    {
        var tasks = _options.Snmp.Targets.Select(target =>
            PollTarget(target, cancellationToken)).ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task PollTarget(SnmpTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(target.IpAddress), target.Port);
            var community = new OctetString(target.Community);
            var timeout = TimeSpan.FromSeconds(_options.Snmp.TimeoutSeconds);

            var oids = target.Oids.Any() ? target.Oids : SnmpProfiles.GetOidsForDeviceType(target.DeviceType);
            var variableList = oids.Select(oid => new Variable(new ObjectIdentifier(oid))).ToList();

            var snmpData = new Dictionary<string, object>();

            // Perform SNMP GET requests
            foreach (var variable in variableList)
            {
                try
                {
                    var result = await Task.Run(() =>
                        Messenger.Get(VersionCode.V2, endpoint, community,
                            new List<Variable> { variable }, _options.Snmp.TimeoutSeconds * 1000), cancellationToken);

                    if (result.Any())
                    {
                        var oidString = variable.Id.ToString();
                        var value = result[0].Data;

                        // Map OID to friendly name
                        var friendlyName = oidString.GetFriendlyName();
                        snmpData[friendlyName] = ParseSnmpValue(value);

                        _logger.LogDebug("SNMP {Target}: {OidName} = {Value}",
                            target.IpAddress, friendlyName, snmpData[friendlyName]);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to query OID {Oid} from {Target}",
                        variable.Id, target.IpAddress);
                }
            }

            // Process the collected data
            if (snmpData.Any())
            {
                var trafficEvents = await _trafficProcessor.ProcessSnmpData(target.IpAddress, snmpData);

                foreach (var trafficEvent in trafficEvents)
                {
                    _eventQueue.Enqueue(trafficEvent);
                }

                _logger.LogDebug("Collected {DataCount} SNMP values from {Target}, generated {EventCount} events",
                    snmpData.Count, target.IpAddress, trafficEvents.Count);

                // If queue is getting full, flush immediately
                if (_eventQueue.Count >= _options.Processing.BufferSize)
                {
                    await FlushEventsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling SNMP target {Target}:{Port}",
                target.IpAddress, target.Port);
        }
    }

    private static object ParseSnmpValue(ISnmpData snmpData) =>
        snmpData switch
        {
            Integer32 int32 => int32.ToInt32(),
            Counter32 counter32 => counter32.ToUInt32(),
            Counter64 counter64 => counter64.ToUInt64(),
            Gauge32 gauge32 => gauge32.ToUInt32(),
            TimeTicks timeTicks => timeTicks.ToUInt32(),
            OctetString octetString => octetString.ToString(),
            ObjectIdentifier objectId => objectId.ToString(),
            IP ipAddress => ipAddress.ToString(),
            Null => null,
            NoSuchObject => null,
            NoSuchInstance => null,
            EndOfMibView => null,
            _ => snmpData.ToString()
        };

    private void FlushEvents(object? state)
    {
        _ = Task.Run(FlushEventsAsync);
    }

    private async Task FlushEventsAsync()
    {
        var events = new List<NetworkTrafficEvent>();

        while (_eventQueue.TryDequeue(out var trafficEvent) && events.Count < _options.Kafka.BatchSize)
        {
            events.Add(trafficEvent);
        }

        if (events.Count > 0)
        {
            await _kafkaProducer.ProduceAsync(events);
            _logger.LogDebug("Flushed {Count} SNMP events to Kafka", events.Count);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping SNMP collector...");

        _flushTimer?.Dispose();

        // Flush remaining events
        await FlushEventsAsync();

        await base.StopAsync(cancellationToken);
    }
}

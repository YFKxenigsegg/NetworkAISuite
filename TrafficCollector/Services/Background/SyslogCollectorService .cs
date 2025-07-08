using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using TrafficCollector.Services.Core.Interfaces;
using TrafficCollector.Models.Events;
using TrafficCollector.Models.Syslog;
using TrafficCollector.Services.ErrorHandling;

namespace TrafficCollector.Services.Background;

public class SyslogCollectorService : BackgroundService
{
    private readonly TrafficCollectorOptions _options;
    private readonly ITrafficProcessor _trafficProcessor;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<SyslogCollectorService> _logger;
    private readonly IErrorHandler _errorHandler;

    private readonly Channel<NetworkTrafficEvent> _eventChannel;
    private readonly ChannelWriter<NetworkTrafficEvent> _eventWriter;
    private readonly ChannelReader<NetworkTrafficEvent> _eventReader;

    private UdpClient? _udpListener;
    private TcpListener? _tcpListener;

    public SyslogCollectorService(
        IOptions<TrafficCollectorOptions> options,
        ITrafficProcessor trafficProcessor,
        IKafkaProducer kafkaProducer,
        ILogger<SyslogCollectorService> logger,
        IErrorHandler errorHandler)
    {
        _options = options.Value;
        _trafficProcessor = trafficProcessor;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _errorHandler = errorHandler;

        // Use channels for high-performance producer/consumer pattern
        var channelOptions = new BoundedChannelOptions(_options.Processing.BufferSize * 2)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // Drop old events if buffer is full
            SingleReader = false,
            SingleWriter = false
        };

        _eventChannel = Channel.CreateBounded<NetworkTrafficEvent>(channelOptions);
        _eventWriter = _eventChannel.Writer;
        _eventReader = _eventChannel.Reader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var tasks = new List<Task>
            {
                // Event processing task
                ProcessEventQueue(stoppingToken)
            };

            if (_options.Syslog.EnableUdp)
            {
                tasks.Add(StartUdpListener(stoppingToken));
            }

            if (_options.Syslog.EnableTcp)
            {
                tasks.Add(StartTcpListener(stoppingToken));
            }

            _logger.LogInformation("High-volume syslog collector started");
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("High-volume syslog collector stopped");
        }
        catch (Exception ex)
        {
            _errorHandler.LogError(ex, "HighVolumeSyslogCollectorService.ExecuteAsync");
            throw;
        }
    }

    private async Task StartUdpListener(CancellationToken cancellationToken)
    {
        _udpListener = new UdpClient(_options.Syslog.Port);
        _logger.LogInformation("High-performance UDP listener started on port {Port}", _options.Syslog.Port);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpListener.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);
                var sourceIp = result.RemoteEndPoint.Address.ToString();

                // Fire and forget - don't await to maintain high throughput
                _ = Task.Run(() => ProcessSyslogMessageFast(message, sourceIp), cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "UDP message receive");
            }
        }
    }

    private async Task ProcessEventQueue(CancellationToken cancellationToken)
    {
        var events = new List<NetworkTrafficEvent>();

        await foreach (var evt in _eventReader.ReadAllAsync(cancellationToken))
        {
            events.Add(evt);

            // Batch events for efficient Kafka publishing
            if (events.Count >= _options.Kafka.BatchSize)
            {
                await FlushEventsBatch(events);
                events.Clear();
            }
        }

        // Flush remaining events
        if (events.Count > 0)
        {
            await FlushEventsBatch(events);
        }
    }

    private void ProcessSyslogMessageFast(string rawMessage, string sourceIp) =>
        _errorHandler.SafeExecuteVoid(() =>
        {
            var syslogMessage = new SyslogMessage
            {
                SourceIp = sourceIp,
                Message = rawMessage,
                Timestamp = DateTime.UtcNow
            };

            var trafficEvent = _trafficProcessor.ProcessSyslogMessage(syslogMessage).Result;

            if (trafficEvent != null)
            {
                // Try to enqueue, if channel is full it will drop oldest
                _eventWriter.TryWrite(trafficEvent);
            }

        }, "ProcessSyslogMessageFast");

    private async Task FlushEventsBatch(List<NetworkTrafficEvent> events) =>
        await _errorHandler.SafeExecuteAsync(async () =>
        {
            await _kafkaProducer.ProduceAsync(events);
            _logger.LogDebug("Flushed {Count} events to Kafka", events.Count);
            return Task.CompletedTask;
        }, "FlushEventsBatch", Task.CompletedTask);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping high-volume syslog collector...");

        _eventWriter.Complete();
        _udpListener?.Close();
        _tcpListener?.Stop();

        await base.StopAsync(cancellationToken);
    }
    
    private async Task StartTcpListener(CancellationToken cancellationToken)
    {
    _tcpListener = new TcpListener(IPAddress.Parse(_options.Syslog.BindAddress), _options.Syslog.Port);
    _tcpListener.Start();
    
    _logger.LogInformation("High-performance TCP listener started on {BindAddress}:{Port}", 
        _options.Syslog.BindAddress, _options.Syslog.Port);

    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync();
            
            // Fire and forget - don't await to maintain high throughput
            _ = Task.Run(() => HandleTcpClientFast(tcpClient, cancellationToken), cancellationToken);
        }
        catch (ObjectDisposedException)
        {
            break;
        }
        catch (Exception ex)
        {
            _errorHandler.LogError(ex, "TCP connection accept");
        }
    }
    }

    private async Task HandleTcpClientFast(TcpClient client, CancellationToken cancellationToken)
    {
    var sourceIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();
    
    try
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        {
            var buffer = new char[_options.Syslog.MaxMessageSize];
            
            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                var message = new string(buffer, 0, bytesRead);
                var messages = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var msg in messages)
                {
                    // Fire and forget for each message
                    _ = Task.Run(() => ProcessSyslogMessageFast(msg.Trim(), sourceIp), cancellationToken);
                }
            }
        }
    }
    catch (Exception ex)
    {
        _errorHandler.LogError(ex, $"TCP client handling [{sourceIp}]");
    }
    }
}

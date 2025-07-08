using TrafficCollector.Models.External;
using TrafficCollector.Services.Core.Interfaces;

namespace TrafficCollector.Services.Core;

public class GeoIpService(
    HttpClient httpClient,
    ILogger<GeoIpService> logger)
    : IGeoIpService, IDisposable
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<GeoIpService> _logger = logger;
    private readonly ConcurrentDictionary<string, (string Country, DateTime CachedAt)> _cache = [];
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> GetCountryAsync(string ipAddress)
    {
        var locationData = await GetLocationDataAsync(ipAddress);
        return locationData?.Country ?? "Unknown";
    }

    public async Task<GeoIpResponse?> GetLocationDataAsync(string ipAddress)
    {
        // Check cache first
        if (_cache.TryGetValue(ipAddress, out var cached))
        {
            if (DateTime.UtcNow - cached.CachedAt < _cacheExpiry)
            {
                return new GeoIpResponse { Country = cached.Country, Status = "success" };
            }
            _cache.TryRemove(ipAddress, out _);
        }

        // Skip private/local IP addresses
        if (IsPrivateIpAddress(ipAddress))
        {
            var localResult = new GeoIpResponse { Country = "Local", Status = "success" };
            _cache[ipAddress] = ("Local", DateTime.UtcNow);
            return localResult;
        }

        try
        {
            // Using ip-api.com with more fields
            var url = $"http://ip-api.com/json/{ipAddress}?fields=status,country,countryCode,region,city,isp";
            
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GeoIP API returned {StatusCode} for {IpAddress}", 
                    response.StatusCode, ipAddress);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var geoData = JsonSerializer.Deserialize<GeoIpResponse>(jsonResponse, _jsonOptions);

            if (geoData?.Status == "success" && !string.IsNullOrEmpty(geoData.Country))
            {
                // Cache the result
                _cache[ipAddress] = (geoData.Country, DateTime.UtcNow);
                _logger.LogDebug("GeoIP lookup successful: {IpAddress} -> {Country}", 
                    ipAddress, geoData.Country);
                return geoData;
            }

            _logger.LogDebug("GeoIP lookup failed for {IpAddress}: {Status}", 
                ipAddress, geoData?.Status ?? "null response");
            return null;
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("GeoIP lookup timeout for {IpAddress}", ipAddress);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse GeoIP response for {IpAddress}", ipAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GeoIP lookup failed for {IpAddress}", ipAddress);
            return null;
        }
    }

    private static bool IsPrivateIpAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            return true; // Treat invalid IPs as private
        }

        var bytes = ip.GetAddressBytes();
        
        return ip.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => 
                // IPv4 private ranges
                bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168) ||
                bytes[0] == 127 || // localhost
                bytes[0] == 169 && bytes[1] == 254, // link-local
            
            System.Net.Sockets.AddressFamily.InterNetworkV6 =>
                // IPv6 private ranges
                ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || 
                IPAddress.IsLoopback(ip) || ip.ToString().StartsWith("fc") || 
                ip.ToString().StartsWith("fd"),
            
            _ => true
        };
    }

    public void Dispose()
    {
        _cache.Clear();
        GC.SuppressFinalize(this);
    }
}

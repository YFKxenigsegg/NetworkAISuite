using TrafficCollector.Models.External;

namespace TrafficCollector.Services.Core.Interfaces;

/// <summary>
/// Service responsible for geographic IP address lookups and location data enrichment
/// </summary>
/// <remarks>
/// Provides geolocation information for IP addresses using external GeoIP services or databases.
/// Includes intelligent caching and private IP address detection to optimize performance and reduce API calls.
/// Used primarily for enriching network traffic events with geographic context for security analysis.
/// </remarks>
public interface IGeoIpService
{
    /// <summary>
    /// Retrieves the country name associated with the specified IP address
    /// </summary>
    /// <param name="ipAddress">The IP address to lookup (IPv4 or IPv6 format)</param>
    /// <returns>Country name if found, "Local" for private IPs, "Unknown" for failed lookups</returns>
    /// <remarks>
    /// This is a convenience method that extracts only the country from the full location data.
    /// Private/internal IP addresses (192.168.x.x, 10.x.x.x, etc.) return "Local" without external lookup.
    /// Invalid IP addresses return "Unknown" without attempting external resolution.
    /// Results are cached to minimize external API calls and improve performance.
    /// Includes timeout protection (typically 5 seconds) to prevent blocking operations.
    /// </remarks>
    Task<string> GetCountryAsync(string ipAddress);

    /// <summary>
    /// Retrieves comprehensive geographic and network information for the specified IP address
    /// </summary>
    /// <param name="ipAddress">The IP address to lookup (IPv4 or IPv6 format)</param>
    /// <returns>GeoIpResponse object with detailed location data, or null if lookup fails</returns>
    /// <remarks>
    /// Provides detailed information including:
    /// - Country and country code
    /// - Region/state and city
    /// - Internet Service Provider (ISP)
    /// - Status indicators for successful/failed lookups
    /// 
    /// Private IP addresses are handled locally and return a response with Country="Local".
    /// External API failures return null rather than throwing exceptions.
    /// Results are cached with configurable expiration (typically 24 hours) to reduce API usage.
    /// Respects rate limits of external GeoIP services (e.g., 15 requests/minute for free tiers).
    /// Used when detailed geographic context is needed for security analysis or traffic categorization.
    /// </remarks>
    Task<GeoIpResponse?> GetLocationDataAsync(string ipAddress);
}

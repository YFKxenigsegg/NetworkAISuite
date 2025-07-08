namespace TrafficCollector.Models.External;

/// <summary>
/// Represents the response data from external GeoIP service lookups containing geographic and network information
/// </summary>
/// <remarks>
/// This model maps to the JSON response format from GeoIP APIs (such as ip-api.com).
/// Used to capture detailed location and network provider information for IP address enrichment.
/// All properties have default empty string values to ensure safe handling of partial API responses.
/// </remarks>
public class GeoIpResponse
{
    /// <summary>
    /// Gets or sets the status of the GeoIP lookup operation
    /// </summary>
    /// <value>
    /// "success" if the lookup was successful and data is available,
    /// "fail" if the lookup failed due to invalid IP or service unavailability,
    /// Empty string for uninitialized or local IP responses
    /// </value>
    /// <remarks>
    /// Used to determine if the other properties contain valid data from the external service.
    /// Local/private IP addresses may have custom status handling.
    /// </remarks>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full country name where the IP address is geographically located
    /// </summary>
    /// <value>
    /// Full country name (e.g., "United States", "Germany", "Japan"),
    /// "Local" for private/internal IP addresses,
    /// Empty string if country information is unavailable
    /// </value>
    /// <remarks>
    /// Primary field used for geographic enrichment of network traffic events.
    /// Country names are returned in English from most GeoIP services.
    /// </remarks>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 country code
    /// </summary>
    /// <value>
    /// Two-letter country code (e.g., "US", "DE", "JP"),
    /// Empty string if country code is unavailable
    /// </value>
    /// <remarks>
    /// Standardized country identifier useful for consistent geographic analysis and filtering.
    /// Follows ISO 3166-1 alpha-2 standard for international country codes.
    /// </remarks>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region, state, or province name where the IP address is located
    /// </summary>
    /// <value>
    /// Region/state name (e.g., "California", "Bavaria", "Tokyo"),
    /// Empty string if regional information is unavailable or not provided by the service
    /// </value>
    /// <remarks>
    /// Provides more granular geographic location than country-level data.
    /// Regional naming conventions vary by country and GeoIP service provider.
    /// </remarks>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city name where the IP address is geographically located
    /// </summary>
    /// <value>
    /// City name (e.g., "Los Angeles", "Munich", "Tokyo"),
    /// Empty string if city information is unavailable or not accurate enough to report
    /// </value>
    /// <remarks>
    /// Most granular geographic location typically available from GeoIP services.
    /// Accuracy varies significantly based on IP address type and geographic region.
    /// Mobile and dynamic IP addresses may have less accurate city-level data.
    /// </remarks>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Internet Service Provider (ISP) or organization name associated with the IP address
    /// </summary>
    /// <value>
    /// ISP or organization name (e.g., "Comcast Cable", "Google LLC", "Amazon Technologies"),
    /// Empty string if ISP information is unavailable
    /// </value>
    /// <remarks>
    /// Useful for identifying traffic sources, detecting cloud providers, and network analysis.
    /// Can help distinguish between residential, business, mobile, and cloud infrastructure traffic.
    /// ISP names may vary in format and detail between different GeoIP service providers.
    /// </remarks>
    public string Isp { get; set; } = string.Empty;
}

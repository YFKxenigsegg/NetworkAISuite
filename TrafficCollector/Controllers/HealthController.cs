using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TrafficCollector.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _healthCheckService.CheckHealthAsync();
        
        var response = new
        {
            Status = result.Status.ToString(),
            Duration = result.TotalDuration,
            Checks = result.Entries.Select(kv => new
            {
                Name = kv.Key,
                Status = kv.Value.Status.ToString(),
                kv.Value.Description,
                kv.Value.Duration,
                kv.Value.Data
            })
        };

        var statusCode = result.Status switch
        {
            HealthStatus.Healthy => 200,
            HealthStatus.Degraded => 200,
            HealthStatus.Unhealthy => 503,
            _ => 500
        };

        return StatusCode(statusCode, response);
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var result = await _healthCheckService.CheckHealthAsync();
        return Ok(result);
    }
}
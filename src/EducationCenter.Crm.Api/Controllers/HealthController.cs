using EducationCenter.Crm.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckResponse>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var response = new HealthCheckResponse(
            report.Status.ToString(),
            report.TotalDuration.TotalMilliseconds);

        var apiResponse = ApiResponse<HealthCheckResponse>.Ok(response, "Hệ thống sẵn sàng.");

        return report.Status == HealthStatus.Healthy
            ? Ok(apiResponse)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, apiResponse);
    }
}

public sealed record HealthCheckResponse(string Status, double DurationMs);

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PlayStudios.Leaderboard.API.Controllers
{
    /// <summary>
    /// API controller for health checks and liveness endpoints.
    /// Used by orchestrators (Kubernetes, ECS, Fargate) to verify service status.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - <c>GET /health/live</c> : liveness probe – is the process running.
    /// - <c>GET /health/ready</c> : readiness probe – is the app ready to accept traffic (DB connection ok, etc).
    /// 
    /// Responsibilities:
    /// - Lightweight, fast responses (no heavy DB calls for liveness).
    /// - Integration with <see cref="Microsoft.Extensions.Diagnostics.HealthChecks"/>.
    /// </remarks>

    [ApiController]
    [Route("api/health")]
    [ApiExplorerSettings(GroupName = "x-health")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        [HttpGet("live")]
        public IActionResult Live() => Ok(new { status = "Healthy" });


        [HttpGet("ready")]
        public async Task<IActionResult> Ready([FromServices] HealthCheckService hc)
        {
            var report = await hc.CheckHealthAsync(r => r.Tags.Contains("ready"));
            var payload = new
            {
                status = report.Status.ToString(),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                entries = report.Entries.ToDictionary(
            kv => kv.Key,
            kv => new {
                status = kv.Value.Status.ToString(),
                description = kv.Value.Description,
                durationMs = kv.Value.Duration.TotalMilliseconds,
                tags = kv.Value.Tags
            })
            };
            return Ok(payload);
        }
    }
}

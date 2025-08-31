using Microsoft.AspNetCore.Mvc;
using PlayStudios.Leaderboard.API.DTOs;
using PlayStudios.Leaderboard.API.Services;

namespace PlayStudios.Leaderboard.API.Controllers
{
    /// <summary>
    /// API controller exposing leaderboard operations.
    /// Wraps the <see cref="IScoreService"/> service layer and translates HTTP requests into business calls.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - <c>POST /api/submit</c> : submit a score for a player.
    /// - <c>GET  /api/leaderboard?playerId=...</c> : get leaderboard snapshot (player rank, top list, nearby list).
    /// - <c>POST /api/reset</c> : clear all scores (admin/test only).
    /// 
    /// Responsibilities:
    /// - Model binding (e.g., <see cref=\"SubmitScoreRequest\"/>).
    /// - Input validation and error responses (400 for invalid input, 500 for server errors).
    /// - Delegates business logic to <see cref=\"IScoreService\"/>.
    /// </remarks>

    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class LeaderboardController : ControllerBase
    {
        private readonly IScoreService _service;
        public LeaderboardController(IScoreService service) => _service = service;


        /// <summary>Submit a player's score (Replace/Accumulate as configured).</summary>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(LeaderboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Submit([FromBody] SubmitScoreRequest request, CancellationToken ct)
        {

            // extra guard: trim and check again 
            if (string.IsNullOrWhiteSpace(request.PlayerId) || request.PlayerId.Trim().Length == 0 || request.PlayerId.Trim().Length > 128)
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [nameof(request.PlayerId)] = new[] { "PlayerId is required and must be <= 128 characters." }
                }));
            }
            // if Score < 0 then throw error
            if (request.Score < 0)
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [nameof(request.Score)] = new[] { "Score must be non-negative." }
                }));
            }

            // ModelState invalid -> auto 400 due to [ApiController]
            var result = await _service.SubmitAsync(request.PlayerId.Trim(), request.Score, ct);
            return Ok(result);
        }


        /// <summary>Get leaderboard summary for a player (top + nearby window).</summary>
        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(LeaderboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] string playerId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(playerId) || playerId.Trim().Length > 128)
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                { ["playerId"] = new[] { "playerId is required and must be <= 128 characters." } }));

            var result = await _service.GetLeaderboardAsync(playerId.Trim(), ct);
            return Ok(result);
        }


        /// <summary>Reset all scores (demo only).</summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> Reset(CancellationToken ct)
        {
            await _service.ResetAsync(ct);
            return Ok(new { Message = "Leaderboard reset" });
        }
    }
}


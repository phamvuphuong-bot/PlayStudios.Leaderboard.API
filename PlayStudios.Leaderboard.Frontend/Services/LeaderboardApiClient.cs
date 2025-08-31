using PlayStudios.Leaderboard.Frontend.Models;

namespace PlayStudios.Leaderboard.Frontend.Services
{

    public class ApiError : Exception { public ApiError(string message) : base(message) { } }

    /// <summary>
    /// Typed HttpClient wrapper for calling the Leaderboard API from Blazor frontend.
    /// Provides high-level methods for Submit, GetBoard, and Reset.
    /// </summary>
    /// <remarks>
    /// Responsibilities:\n
    /// - Encapsulates raw HTTP calls and JSON serialization.\n
    /// - Throws <see cref="ApiError"/> with descriptive message if the API returns non-success.\n
    /// - Ensures Blazor components can consume simple C# methods without worrying about HttpClient details.\n
    /// 
    /// Usage:\n
    /// Registered in DI container via <c>builder.Services.AddHttpClient&lt;LeaderboardApiClient&gt;(...)</c>.\n
    /// Injected into Razor components with <c>@inject LeaderboardApiClient Api</c>.
    /// </remarks>
    public class LeaderboardApiClient
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Constructs the API client with injected <see cref="HttpClient"/>.
        /// </summary>
        public LeaderboardApiClient(HttpClient http) => _http = http;

        /// <summary>
        /// Submit a new score for the given player.
        /// </summary>
        /// <param name="playerId">Unique id of the player.</param>
        /// <param name="score">Score value (non-negative).</param>
        /// <returns><see cref="LeaderboardResponse"/> snapshot with updated rank and board state.</returns>
        /// <exception cref="ApiError">Thrown if API response is not successful (non-2xx).</exception>
        public async Task<LeaderboardResponse> SubmitAsync(string playerId, long score, CancellationToken ct = default)
        {
            var payload = new SubmitScoreRequest { PlayerId = playerId, Score = score };
            var res = await _http.PostAsJsonAsync("/api/submit", payload, ct);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new ApiError($"Submit failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }
            return (await res.Content.ReadFromJsonAsync<LeaderboardResponse>(cancellationToken: ct))!;
        }

        /// <summary>
        /// Get the leaderboard snapshot for a given player.
        /// </summary>
        /// <param name="playerId">Player to query.</param>
        /// <returns>Snapshot including player's rank, top list, and nearby list.</returns>
        /// <exception cref="ApiError">Thrown if API response is not successful.</exception>

        public async Task<LeaderboardResponse> GetBoardAsync(string playerId, CancellationToken ct = default)
        {
            var res = await _http.GetAsync($"/api/leaderboard?playerId={Uri.EscapeDataString(playerId)}", ct);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new ApiError($"Get failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }
            return (await res.Content.ReadFromJsonAsync<LeaderboardResponse>(cancellationToken: ct))!;
        }

        /// <summary>
        /// Reset all scores (admin/test use).
        /// </summary>
        /// <exception cref="ApiError">Thrown if API response is not successful.</exception>
        public async Task ResetAsync(CancellationToken ct = default)
        {
            var res = await _http.PostAsync("/api/reset", content: null, ct);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                throw new ApiError($"Reset failed: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }
        }
    }
}

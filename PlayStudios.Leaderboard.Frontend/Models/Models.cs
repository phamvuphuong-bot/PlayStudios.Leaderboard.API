namespace PlayStudios.Leaderboard.Frontend.Models
{
    /// <summary>
    /// Request DTO for submitting a player's score from frontend to API.
    /// </summary>
    /// <remarks>
    /// Mirrors the API contract for <c>POST /api/submit</c>.
    /// Used only as payload when calling <see cref="LeaderboardApiClient.SubmitAsync"/>.
    /// </remarks>
    public class SubmitScoreRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public long Score { get; set; }
    }

    /// <summary>
    /// Lightweight DTO representing a single row in leaderboard results.
    /// </summary>
    /// <remarks>
    /// Same structure as server-side DTO, used for both TopPlayers and NearbyPlayers lists.\n
    /// Contains only minimal fields for display: PlayerId, Score, Rank.\n
    /// Immutable record type for easy serialization and UI binding.
    /// </remarks>
    public record LeaderboardPlayerDto(string PlayerId, long Score, long Rank);

    /// <summary>
    /// Response DTO returned by API calls to leaderboard endpoints.
    /// </summary>
    /// <remarks>
    /// Contains a snapshot for a given player:\n
    /// - PlayerId, PlayerScore, PlayerRank\n
    /// - TopPlayers (global top N)\n
    /// - NearbyPlayers (window around the player)\n
    /// Used by Blazor UI to render tables and badges.
    /// </remarks>
    public class LeaderboardResponse
    {
        public string PlayerId { get; set; } = string.Empty;
        public long PlayerRank { get; set; }
        public long PlayerScore { get; set; }
        public List<LeaderboardPlayerDto> TopPlayers { get; set; } = new();
        public List<LeaderboardPlayerDto> NearbyPlayers { get; set; } = new();
    }
}

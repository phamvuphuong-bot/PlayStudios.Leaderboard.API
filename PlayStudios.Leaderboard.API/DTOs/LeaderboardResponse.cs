namespace PlayStudios.Leaderboard.API.DTOs
{
    /// <summary>
    /// Data transfer object returned by leaderboard endpoints.
    /// Encapsulates a snapshot of the leaderboard state for a given player.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// - PlayerId, PlayerScore, PlayerRank for the requested player.
    /// - TopPlayers: top N players globally.
    /// - NearbyPlayers: players close to the requested player's rank.
    /// This class is designed to be serialized as JSON and consumed by clients (Swagger, frontend).
    /// </remarks>
    public class LeaderboardResponse
    {
        public string ResultString { get; set; } = default!;
        /// <summary>Unique identifier of the player for whom this response is generated.</summary>
        public string PlayerId { get; set; } = default!;


        /// <summary>The current rank of the player (1 = highest).</summary>
        public long PlayerRank { get; set; }

        /// <summary>The latest score of the player.</summary>
        public long PlayerScore { get; set; }

        /// <summary>Top N players globally, determined by score and rank.</summary>
        public List<LeaderboardPlayerDto> TopPlayers { get; set; } = new();

        /// <summary>Players ranked near the requested player, for context.</summary>
        public List<LeaderboardPlayerDto> NearbyPlayers { get; set; } = new();

       
    }
}

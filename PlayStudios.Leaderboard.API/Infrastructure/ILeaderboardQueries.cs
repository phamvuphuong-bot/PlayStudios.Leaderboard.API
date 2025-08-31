using PlayStudios.Leaderboard.API.Domain;

namespace PlayStudios.Leaderboard.API.Infrastructure
{
    /// <summary>
    /// Read-only query abstraction for leaderboard views (ranking, top list, nearby window).
    /// Uses SQL window functions and keyless projections to read efficiently without tracking.
    /// </summary>
    /// <remarks>
    /// Implementations should:
    /// - Use parameterized SQL (FromSqlRaw/FromSqlInterpolated) to avoid injection.
    /// - Return raw projections (e.g., <see cref="RankedPlayer"/>) instead of DTOs to keep layers decoupled.
    /// - Prefer <c>AsNoTracking()</c> for query performance.
    /// </remarks>
    public interface ILeaderboardQueries
    {
        /// <summary>Gets the rank and score of a player (dense rank over Score desc).</summary>
        Task<(long rank, long score)> GetPlayerRankAsync(string playerId, CancellationToken ct = default);

        /// <summary>Gets the top N ranked players as raw projections.</summary>
        Task<List<RankedPlayer>> GetTopAsync(int top, CancellationToken ct = default);

        /// <summary>Gets players in a rank window centered around <paramref name="targetRank"/> (inclusive).</summary>
        Task<List<RankedPlayer>> GetNearbyAsync(long targetRank, int range, CancellationToken ct = default);
    }
}

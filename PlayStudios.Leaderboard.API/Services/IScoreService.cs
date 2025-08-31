using PlayStudios.Leaderboard.API.DTOs;



namespace PlayStudios.Leaderboard.API.Services
{
    /// <summary>
    /// Defines the contract for leaderboard operations.
    /// Service layer ensures business logic is separate from controllers and repositories.
    /// </summary>

    public interface IScoreService
    {
        /// <summary>
        /// Submit a new score for a given player.
        /// Business rules applied here (replace or update score depending on config).
        /// </summary>
        /// <param name="playerId">Unique identifier of the player.</param>
        /// <param name="score">New score to submit.</param>
        /// <returns>A response including player's rank, score, top players and nearby players.</returns>
        Task<LeaderboardResponse> SubmitAsync(string playerId, long score, CancellationToken ct = default);

        /// <summary>
        /// Get the leaderboard snapshot for a given player.
        /// Includes player's rank + top list + nearby list.
        /// </summary>
        /// <param name="playerId">Player to center the leaderboard around.</param>
        /// <returns>Leaderboard response with rankings.</returns>
        Task<LeaderboardResponse> GetLeaderboardAsync(string playerId, CancellationToken ct = default);

        /// <summary>
        /// Reset all scores in the system.
        /// Useful for testing or refreshing leaderboard state.
        /// </summary>
        Task ResetAsync(CancellationToken ct = default);
    }
}

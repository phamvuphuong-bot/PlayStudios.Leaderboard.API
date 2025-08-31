using PlayStudios.Leaderboard.API.Domain;

namespace PlayStudios.Leaderboard.API.Infrastructure
{
    /// <summary>
    /// Write-side abstraction for player score persistence (create/update/reset).
    /// Keeps mutation logic isolated from controllers/services and from read models.
    /// </summary>
    /// <remarks>
    /// Implementations should ensure idempotent upserts and handle both Replace and Accumulate modes.
    /// </remarks>
    public interface IPlayerScoreRepository
    {
        /// <summary>Get a Player by ID</summary>
        Task<PlayerScore?> GetAsync(string playerId, CancellationToken ct = default);

        /// <summary>
        /// Insert new or update existing player score.
        /// </summary>
        /// <param name="playerId">Logical key (max length typically 128).</param>
        /// <param name="score">Incoming score value.</param>
        /// <param name="accumulate">If true, add to existing score; otherwise replace.</param>
        Task UpsertAsync(string playerId, long score, bool accumulate, CancellationToken ct = default);

        /// <summary>Deletes all player scores (admin/test helper).</summary>
        Task ResetAsync(CancellationToken ct = default);
    }
}

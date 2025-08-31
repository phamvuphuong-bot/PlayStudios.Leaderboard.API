using System.ComponentModel.DataAnnotations;

namespace PlayStudios.Leaderboard.API.Domain
{
    /// <summary>
    /// Entity model representing a player's persisted score.
    /// Stored in the database via <see cref="AppDbContext"/>.
    /// </summary>
    /// <remarks>
    /// - <see cref="PlayerId"/> is the primary key (string, max length ~128).
    /// - <see cref="Score"/> is the numeric score, updated on submit.
    /// - <see cref="UpdatedAt"/> tracks last modification timestamp.
    /// This is the **write model** of the leaderboard, used by repository layer.
    /// Ranking queries do not track this entity directly but project into keyless types
    /// such as <see cref="RankedPlayer"/>.
    /// </remarks>
    public class PlayerScore
    {
        /// <summary>
        /// Unique identifier of the player.
        /// Primary key in the PlayerScores table.
        /// </summary>
        [Key]
        [StringLength(128, MinimumLength = 1)]
        public string PlayerId { get; set; } = default!;

        /// <summary>
        /// Current score of the player.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long Score { get; set; }

        /// <summary>
        /// Timestamp (UTC) when the score was last updated.
        /// Useful for debugging, audits, or time-based leaderboards.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

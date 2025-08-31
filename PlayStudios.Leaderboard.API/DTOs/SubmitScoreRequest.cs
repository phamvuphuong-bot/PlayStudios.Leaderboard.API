using System.ComponentModel.DataAnnotations;

namespace PlayStudios.Leaderboard.API.DTOs
{
    // DLO (Data Layer Object) / DTOs
    public class SubmitScoreRequest
    {
        /// <summary>Unique player identifier (max 128 chars). Unicode allowed.</summary>
        [Required, StringLength(128, MinimumLength = 1)]
        public string PlayerId { get; init; } = default!;


        /// <summary>Non-negative score (0..long.MaxValue).</summary>
        [Range(0, long.MaxValue)]
        public long Score { get; init; }
    }
}

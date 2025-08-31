using Microsoft.EntityFrameworkCore;

namespace PlayStudios.Leaderboard.API.Domain
{

    /// <summary>
    /// Keyless projection used to materialize ranked rows from raw SQL
    /// (PlayerId, Score, Rank). 
    /// </summary>
    [Keyless]
    public class RankedPlayer
    {
        public string PlayerId { get; set; } = default!;
        public long Score { get; set; }
        public long Rank { get; set; }
    }


    /// <summary>
    /// Keyless projection for a single player's (Rank, Score) lookup.
    /// </summary>
    [Keyless]
    public class RankAndScore
    {
        public long Rank { get; set; }
        public long Score { get; set; }
    }
}

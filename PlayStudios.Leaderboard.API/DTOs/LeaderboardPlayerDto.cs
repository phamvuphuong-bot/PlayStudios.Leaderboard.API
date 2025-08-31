namespace PlayStudios.Leaderboard.API.DTOs
{

    /// <summary>
    /// Lightweight DTO representing a single player in leaderboard queries.
    /// Used in both TopPlayers and NearbyPlayers lists.
    /// </summary>
    /// <remarks>
    /// - Contains only the minimal fields needed for UI: PlayerId, Score, Rank.     
    /// </remarks>
    public class LeaderboardPlayerDto
    {

        public string PlayerId { get; set; } = default!;
        public long Score { get; set; }
        public long Rank { get; set; }

        public LeaderboardPlayerDto(string pid, long score, long rank)         
        { 
            PlayerId = pid;
            Score = score;
            Rank = rank;
        }
    }
}

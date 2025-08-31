using Microsoft.EntityFrameworkCore;
using PlayStudios.Leaderboard.API.Domain;

namespace PlayStudios.Leaderboard.API.Infrastructure
{

    /// <summary>
    /// SQL-backed query implementation for ranking operations.
    /// Computes ranks via SQL window function (<c>DENSE_RANK()</c>) and projects into keyless types.
    /// </summary>
    /// <remarks>
    /// - Uses a CTE to compute ranks once and selects different slices (top, nearby, single-player).
    /// - Returns <see cref="RankedPlayer"/> or tuples for performance and layer separation.
    /// - Applies <c>AsNoTracking()</c> for read-only efficiency and deterministic ordering:
    ///   ORDER BY Rank, PlayerId to keep results stable on ties.
    /// </remarks>
    public class LeaderboardQueries : ILeaderboardQueries
    {
        private readonly AppDbContext _db;
        public LeaderboardQueries(AppDbContext db) => _db = db;


        // Use SQL window function DENSE_RANK over Score DESC
        private const string RankCte = @"
                                        WITH ranked AS (
                                        SELECT PlayerId, Score,
                                        DENSE_RANK() OVER (ORDER BY Score DESC) AS [Rank]
                                        FROM PlayerScores
                                        )
                                        ";


        public async Task<(long rank, long score)> GetPlayerRankAsync(string playerId, CancellationToken ct = default)
        {
            var sql = $"{RankCte} SELECT [Rank], [Score] FROM ranked WHERE PlayerId = @p0";
            var rows = await _db.RankAndScores
            .FromSqlRaw(sql, playerId)
            .AsNoTracking()
            .ToListAsync(ct);

            var row = rows.FirstOrDefault(); 
            if (row == null) return (-1, -1);
            return (row.Rank, row.Score);
        }


        public async Task<List<RankedPlayer>> GetTopAsync(int top, CancellationToken ct = default)
        {
            var sql = $"{RankCte} SELECT TOP(@p0) PlayerId, Score, [Rank] FROM ranked ORDER BY [Rank], PlayerId";
            var rows = await _db.RankedPlayers
            .FromSqlRaw(sql, top)
            .AsNoTracking()
            .ToListAsync(ct);
            return rows;
        }


        public async Task<List<RankedPlayer>> GetNearbyAsync(long targetRank, int range, CancellationToken ct = default)
        {
            var minRank = targetRank - range;
            if (minRank < 1) minRank = 1; // guard
            var maxRank = targetRank + range;
            var sql = $"{RankCte} SELECT PlayerId, Score, [Rank] FROM ranked WHERE [Rank] BETWEEN @p0 AND @p1 ORDER BY [Rank], PlayerId";
            var rows = await _db.RankedPlayers
            .FromSqlRaw(sql, minRank, maxRank)
            .AsNoTracking()
            .ToListAsync(ct);
            return rows;
        }
    }
}

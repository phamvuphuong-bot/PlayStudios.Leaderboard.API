using Microsoft.EntityFrameworkCore;
using PlayStudios.Leaderboard.API.Domain;
using System.Numerics;

namespace PlayStudios.Leaderboard.API.Infrastructure
{
    /// <summary>
    /// EF Core implementation of <see cref="IPlayerScoreRepository"/>.
    /// Performs upserts on <see cref="PlayerScore"/> and persists changes efficiently.
    /// </summary>
    /// <remarks>
    /// - Uses the PlayerId as primary key.
    /// - Replace mode: set <c>Score = incoming</c>.
    /// - Accumulate mode: <c>Score += incoming</c> with basic concurrency safety (last-write-wins by default).
    /// - Updates <c>UpdatedAt</c> on every mutation.
    /// Consider adding optimistic concurrency (rowversion) for stricter guarantees if required.
    /// </remarks>
    public class PlayerScoreRepository : IPlayerScoreRepository
    {
        private readonly AppDbContext _db;
        public PlayerScoreRepository(AppDbContext db) => _db = db;


        public async Task<PlayerScore?> GetAsync(string playerId, CancellationToken ct = default)
        {
            return await _db.PlayerScores.AsNoTracking().FirstOrDefaultAsync(x => x.PlayerId == playerId, ct);
        }


        public async Task UpsertAsync(string playerId, long score, bool accumulate, CancellationToken ct = default)
        {
            var entity = await _db.PlayerScores.FirstOrDefaultAsync(x => x.PlayerId == playerId, ct);
            if (entity == null)
            {
                entity = new PlayerScore { PlayerId = playerId, Score = score, UpdatedAt = DateTime.UtcNow };
                _db.PlayerScores.Add(entity);
            }
            else
            {
                entity.Score = accumulate ? entity.Score + score : score;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }


        public async Task ResetAsync(CancellationToken ct = default)
        {
            // Truncate-like reset: delete all rows
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM PlayerScores", ct);
        }
    }
}

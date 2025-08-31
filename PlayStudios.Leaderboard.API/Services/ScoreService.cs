using Microsoft.Extensions.Options;
using PlayStudios.Leaderboard.API.Config;
using PlayStudios.Leaderboard.API.DTOs;
using PlayStudios.Leaderboard.API.Infrastructure;

namespace PlayStudios.Leaderboard.API.Services
{
    /// <summary>
    /// Implements IScoreService and contains the main business logic for leaderboard.
    /// </summary>
    public class ScoreService : IScoreService
    {
        private readonly IPlayerScoreRepository _repo;
        private readonly ILeaderboardQueries _queries;
        private readonly LeaderboardSettings _settings;

        /// <summary>
        /// Initializes the ScoreService.
        /// </summary>
        /// <param name="repo">Repository abstraction for writing player scores.</param>
        /// <param name="queries">Query abstraction for reading rankings from database.</param>
        /// <param name="settings">Configurable leaderboard settings (TopLimit, NearbyRange, UpdateMode).</param>

        public ScoreService(IPlayerScoreRepository repo, ILeaderboardQueries queries, IOptions<LeaderboardSettings> settings)
        {
            _repo = repo;
            _queries = queries;
            _settings = settings.Value;
        }

        // 1. Apply business rule: replace or update score
        // 2. Save to repository
        // 3. Query DB for player's rank, top players, nearby players
        // 4. Map to DTO response
        public async Task<LeaderboardResponse> SubmitAsync(string playerId, long score, CancellationToken ct = default)
        {
            var pid = (playerId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pid)) throw new ArgumentException("playerId is required", nameof(playerId));
            bool accumulate = string.Equals(_settings.UpdateMode, "Accumulate", StringComparison.OrdinalIgnoreCase);
            await _repo.UpsertAsync(pid, score, accumulate, ct);
            return await BuildResponse(pid, ct);
        }

        // 1. Query player's rank
        // 2. Query top and nearby lists
        // 3. Map to response DTO
        public async Task<LeaderboardResponse> GetLeaderboardAsync(string playerId, CancellationToken ct = default)
        {
            var pid = (playerId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pid)) throw new ArgumentException("playerId is required", nameof(playerId));
            return await BuildResponse(pid, ct);
        }

        // Clears all data from the repository
        public async Task ResetAsync(CancellationToken ct = default)
        {
            await _repo.ResetAsync(ct);
        }

        // 1. Query DB for player's rank, top players, nearby players
        // 2. Map to LeaderboardResponse
        private async Task<LeaderboardResponse> BuildResponse(string playerId, CancellationToken ct)
        {
            var topLimit = Math.Max(0, _settings.TopLimit);
            var nearbyRange = Math.Max(0, _settings.NearbyRange);


            var (rank, score) = await _queries.GetPlayerRankAsync(playerId, ct);


            var topRows = await _queries.GetTopAsync(topLimit, ct);
            var top = topRows.Select(r => new LeaderboardPlayerDto(r.PlayerId, r.Score, r.Rank)).ToList();


            var nearby = new List<LeaderboardPlayerDto>();
            if (rank > 0 && nearbyRange > 0)
            {
                var nearbyRows = await _queries.GetNearbyAsync(rank, nearbyRange, ct);
                nearby = nearbyRows
                .Where(p => p.PlayerId != playerId)
                .Select(r => new LeaderboardPlayerDto(r.PlayerId, r.Score, r.Rank))
                .ToList();
            }


            return new LeaderboardResponse
            {
                PlayerId = playerId,
                PlayerRank = rank,
                PlayerScore = score,
                TopPlayers = top,
                NearbyPlayers = nearby
            };
        }
    }
}

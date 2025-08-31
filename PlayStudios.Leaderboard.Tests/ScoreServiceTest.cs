using Moq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayStudios.Leaderboard.API.Config;
using PlayStudios.Leaderboard.API.Domain;
using PlayStudios.Leaderboard.API.Infrastructure;
using PlayStudios.Leaderboard.API.Services;
using Xunit;
using PlayStudios.Leaderboard.API.DTOs;

namespace PlayStudios.Leaderboard.Tests
{
    /// <summary>
    /// Unit tests for ScoreService (service layer orchestrating repo + queries).
    /// Goals:
    ///  - Verify business rules (Replace/Accumulate).
    ///  - Ensure mapping from infra projections -> DTO is correct.
    ///  - Validate edge behaviors (nearby excludes self, zero ranges).
    /// Strategy:
    ///  - Mock IPlayerScoreRepository (write-side).
    ///  - Mock ILeaderboardQueries (read-side).
    ///  - Inject IOptions<LeaderboardSettings>.
    /// </summary>
    public class ScoreServiceTests
    {

        /// <summary>
        /// Happy path: Submit (Replace) -> Get leaderboard with top & nearby.
        /// Ensures:
        ///  - Player id/score/rank returned correctly.
        ///  - TopPlayers count matches config.
        ///  - NearbyPlayers excludes the requesting player.
        /// </summary>

        [Fact]
        public async Task Submit_Then_GetLeaderboard_Works()
        {
            // Arrange
            var repo = new Mock<IPlayerScoreRepository>();
            var queries = new Mock<ILeaderboardQueries>();
            var settings = Options.Create(new LeaderboardSettings
            {
                TopLimit = 3,
                NearbyRange = 1,
                UpdateMode = "Replace"
            });

            queries.Setup(q => q.GetPlayerRankAsync("A", default))
                   .ReturnsAsync((1L, 100L));

            queries.Setup(q => q.GetTopAsync(3, default))
                   .ReturnsAsync(new List<RankedPlayer>
                   {
                       new() { PlayerId = "A", Score = 100, Rank = 1 },
                       new() { PlayerId = "B", Score = 90,  Rank = 2 },
                       new() { PlayerId = "C", Score = 80,  Rank = 3 },
                   });

            queries.Setup(q => q.GetNearbyAsync(1, 1, default))
                   .ReturnsAsync(new List<RankedPlayer>
                   {
                       new() { PlayerId = "A", Score = 100, Rank = 1 },
                       new() { PlayerId = "B", Score = 90,  Rank = 2 },
                   });

            var svc = new ScoreService(repo.Object, queries.Object, settings);

            // Act
            var resp = await svc.SubmitAsync("A", 100);

            // Assert
            Assert.Equal("A", resp.PlayerId);
            Assert.Equal(1, resp.PlayerRank);
            Assert.Equal(100, resp.PlayerScore);
            Assert.Equal(3, resp.TopPlayers.Count);
            Assert.DoesNotContain(resp.NearbyPlayers, x => x.PlayerId == "A");

            // Verify write called with Replace (accumulate=false)
            repo.Verify(r => r.UpsertAsync("A", 100, false, default), Times.Once);
        }

        /// <summary>
        /// Accumulate mode: submitting twice adds up the score.
        /// We simulate by verifying repository is called with accumulate=true
        /// and that returned snapshot reflects the new score/rank.
        /// </summary>

        [Fact]
        public async Task Submit_WithAccumulate_AddsScore()
        {
            // Arrange
            var repo = new Mock<IPlayerScoreRepository>();
            var queries = new Mock<ILeaderboardQueries>();
            var settings = Options.Create(new LeaderboardSettings
            {
                TopLimit = 2,
                NearbyRange = 0,
                UpdateMode = "Accumulate"
            });

            // After second submit, DB shows total 150 and rank 2
            queries.Setup(q => q.GetPlayerRankAsync("A", default))
                   .ReturnsAsync((2L, 150L));
            queries.Setup(q => q.GetTopAsync(2, default))
                   .ReturnsAsync(new List<RankedPlayer>
                   {
                       new() { PlayerId = "Z", Score = 200, Rank = 1 },
                       new() { PlayerId = "A", Score = 150, Rank = 2 },
                   });

            var svc = new ScoreService(repo.Object, queries.Object, settings);

            // Act
            await svc.SubmitAsync("A", 50);
            var resp = await svc.SubmitAsync("A", 100);

            // Assert
            Assert.Equal("A", resp.PlayerId);
            Assert.Equal(150, resp.PlayerScore);
            Assert.Equal(2, resp.PlayerRank);
            Assert.Empty(resp.NearbyPlayers); // NearbyRange = 0

            // Verify accumulate=true
            repo.Verify(r => r.UpsertAsync("A", 50, true, default), Times.Once);
            repo.Verify(r => r.UpsertAsync("A", 100, true, default), Times.Once);
        }

        /// <summary>
        /// NearbyRange=0 should return an empty NearbyPlayers list (no errors).
        /// </summary>
        [Fact]
        public async Task NearbyRange_Zero_ReturnsEmptyNearby()
        {
            // Arrange
            var repo = new Mock<IPlayerScoreRepository>();
            var queries = new Mock<ILeaderboardQueries>();
            var settings = Options.Create(new LeaderboardSettings
            {
                TopLimit = 1,
                NearbyRange = 0,
                UpdateMode = "Replace"
            });

            queries.Setup(q => q.GetPlayerRankAsync("B", default))
                   .ReturnsAsync((5L, 10L));
            queries.Setup(q => q.GetTopAsync(1, default))
                   .ReturnsAsync(new List<RankedPlayer>
                   {
                       new() { PlayerId = "X", Score = 999, Rank = 1 }
                   });

            var svc = new ScoreService(repo.Object, queries.Object, settings);

            // Act
            var resp = await svc.GetLeaderboardAsync("B");

            // Assert
            Assert.Equal("B", resp.PlayerId);
            Assert.Equal(10, resp.PlayerScore);
            Assert.Equal(5, resp.PlayerRank);
            Assert.Empty(resp.NearbyPlayers);
        }
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayStudios.Leaderboard.API.Infrastructure;


namespace PlayStudios.Leaderboard.Tests
{
    /// <summary>
    /// Repository tests for PlayerScoreRepository using SQLite in-memory.
    /// Covers: insert, replace, accumulate, reset, and UpdatedAt behavior.
    /// </summary>
    public class RepositoryTests : IDisposable
    {
        private readonly SqliteConnection _conn;

        public RepositoryTests()
        {
            // One shared in-memory connection per test class ensures the schema exists for each context.
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open();
            using var ctx = CreateContext();
            ctx.Database.EnsureCreated();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_conn) // relational, supports constraints and SQL
                .EnableSensitiveDataLogging()
                .Options;
            return new AppDbContext(options);
        }

        public void Dispose()
        {
            _conn.Dispose();
        }

        [Fact]
        public async Task Upsert_NewPlayer_Replace_Inserts()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("alice", 120, accumulate: false, CancellationToken.None);

            var row = await ctx.PlayerScores.AsNoTracking().FirstOrDefaultAsync(x => x.PlayerId == "alice");
            Assert.NotNull(row);
            Assert.Equal(120, row!.Score);
            Assert.True((DateTime.UtcNow - row.UpdatedAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task Upsert_Existing_Replace_OverwritesScore_And_UpdatesTimestamp()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("bob", 50, accumulate: false, CancellationToken.None);
            var before = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "bob");
            await Task.Delay(20);

            await repo.UpsertAsync("bob", 200, accumulate: false, CancellationToken.None);
            var after = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "bob");

            Assert.Equal(200, after.Score);
            Assert.True(after.UpdatedAt > before.UpdatedAt);
        }

        [Fact]
        public async Task Upsert_Existing_Accumulate_AddsScore()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("carol", 70, accumulate: true, CancellationToken.None);
            await repo.UpsertAsync("carol", 30, accumulate: true, CancellationToken.None);

            var row = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "carol");
            Assert.Equal(100, row.Score);
        }

        [Fact]
        public async Task Upsert_Accumulate_WithZero_DoesNotDecrease()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("dave", 10, accumulate: true, CancellationToken.None);
            await repo.UpsertAsync("dave", 0, accumulate: true, CancellationToken.None);

            var row = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "dave");
            Assert.Equal(10, row.Score);
        }

        [Fact]
        public async Task Reset_RemovesAllRows()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("p1", 1, false, default);
            await repo.UpsertAsync("p2", 2, false, default);
            await repo.UpsertAsync("p3", 3, false, default);

            Assert.Equal(3, await ctx.PlayerScores.CountAsync());

            await repo.ResetAsync(default);

            Assert.Equal(0, await ctx.PlayerScores.CountAsync());
        }

        [Fact]
        public async Task Upsert_MultiplePlayers_PrimaryKeyIsPlayerId()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            await repo.UpsertAsync("A", 10, false, default);
            await repo.UpsertAsync("B", 20, false, default);
            await repo.UpsertAsync("A", 15, false, default); // replace

            var a = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "A");
            var b = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "B");

            Assert.Equal(15, a.Score);
            Assert.Equal(20, b.Score);
        }

        // Optional: simple concurrency-ish check (last write wins)
        [Fact]
        public async Task Upsert_LastWriteWins_SimpleRace()
        {
            await using var ctx = CreateContext();
            var repo = new PlayerScoreRepository(ctx);

            var t1 = repo.UpsertAsync("race", 100, false, default);
            var t2 = repo.UpsertAsync("race", 200, false, default);
            await Task.WhenAll(t1, t2);

            var row = await ctx.PlayerScores.AsNoTracking().FirstAsync(x => x.PlayerId == "race");
            Assert.True(row.Score is 100 or 200); // last write wins; value is deterministic in real repo if you lock/transaction
        }
    }
}

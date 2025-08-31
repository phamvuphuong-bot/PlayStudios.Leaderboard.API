using Microsoft.EntityFrameworkCore;
using PlayStudios.Leaderboard.API.Domain;

namespace PlayStudios.Leaderboard.API.Infrastructure
{
    /// <summary>
    /// EF Core database context for the Leaderboard service.
    /// Manages the PlayerScores aggregate and exposes keyless projections used by raw SQL queries.
    /// </summary>
    /// <remarks>
    /// - <see cref="PlayerScore"/> is the main entity (key = PlayerId).
    /// - <see cref="RankedPlayer"/> and <see cref="RankAndScore"/> are keyless types mapped to raw SQL results
    ///   (configured via <c>HasNoKey()</c> and <c>ToView(null)</c>).
    /// - Contains basic indexes (e.g., on Score) for ranking queries.
    /// </remarks>
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<PlayerScore> PlayerScores => Set<PlayerScore>();


        // Keyless projections for raw SQL results
        public DbSet<RankedPlayer> RankedPlayers => Set<RankedPlayer>();
        public DbSet<RankAndScore> RankAndScores => Set<RankAndScore>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerScore>(e =>
            {
                e.HasKey(x => x.PlayerId);
                e.Property(x => x.PlayerId).HasMaxLength(128);
                e.HasIndex(x => x.Score).HasDatabaseName("IX_PlayerScores_Score");
            });


            // Configure keyless result types used by FromSqlRaw
            modelBuilder.Entity<RankedPlayer>().HasNoKey().ToView(null);
            modelBuilder.Entity<RankAndScore>().HasNoKey().ToView(null);
        }
    }
}

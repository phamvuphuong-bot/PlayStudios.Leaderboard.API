using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace PlayStudios.Leaderboard.API.Config
{
    // <summary>
    /// Strongly-typed configuration options for the leaderboard.
    /// Bound from configuration (appsettings.json, environment variables).
    /// </summary>
    /// <remarks>
    /// Properties:\n
    /// - <see cref="TopLimit"/> : number of players to return in the Top list (e.g. top 10).\n
    /// - <see cref="NearbyRange"/> : how many ranks above/below the player to include in Nearby list.\n
    /// - <see cref="UpdateMode"/> : business rule for updating scores (e.g. \"Replace\" or \"Accumulate\").\n
    /// 
    /// Usage:\n
    /// Registered via <c>IOptions&lt;LeaderboardSettings&gt;</c> and injected into <see cref="ScoreService"/>.\n
    /// Allows runtime configuration without code changes.
    /// </remarks>
    public class LeaderboardSettings
    {

        /// <summary>How many top players to return. 0 allowed (means none).</summary>
        [Range(0, int.MaxValue)]
        public int TopLimit { get; set; } = 10;


        /// <summary>How many ranks above/below the player to include. 0 allowed.</summary>
        [Range(0, int.MaxValue)]
        public int NearbyRange { get; set; } = 2;


        /// <summary>Hours between automatic resets (not implemented in sample).</summary>
        [Range(0, int.MaxValue)]
        public int ResetIntervalHours { get; set; } = 24;


        /// <summary>Replace | Accumulate</summary>
        [Required]
        public string UpdateMode { get; set; } = "Replace";
    }


    /// <summary>
    /// Stronger validation for settings at startup. Ensures UpdateMode is valid.
    /// </summary>
    public sealed class LeaderboardSettingsValidator : IValidateOptions<LeaderboardSettings>
    {
        public ValidateOptionsResult Validate(string? name, LeaderboardSettings options)
        {
            if (!string.Equals(options.UpdateMode, "Replace", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.UpdateMode, "Accumulate", StringComparison.OrdinalIgnoreCase))
            {
                return ValidateOptionsResult.Fail("Leaderboard:UpdateMode must be either 'Replace' or 'Accumulate'.");
            }
            if (options.TopLimit < 0) return ValidateOptionsResult.Fail("Leaderboard:TopLimit must be >= 0.");
            if (options.NearbyRange < 0) return ValidateOptionsResult.Fail("Leaderboard:NearbyRange must be >= 0.");
            if (options.ResetIntervalHours < 0) return ValidateOptionsResult.Fail("Leaderboard:ResetIntervalHours must be >= 0.");
            return ValidateOptionsResult.Success;
        }
    }

}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PlayStudios.Leaderboard.API.Config;
using PlayStudios.Leaderboard.API.Infrastructure;
using PlayStudios.Leaderboard.API.Services;


/// <summary>
/// Program.cs – entry point of the Leaderboard API.
/// Configures services, middleware, and endpoint mappings.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Configure dependency injection (DbContext, repositories, services, AutoMapper, health checks).
/// - Register middleware (Swagger, HTTPS redirection, routing, authorization).
/// - Map API endpoints (controllers, minimal APIs if any).
/// - Configure environment specific settings (Development vs Production).
/// </remarks>

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Bind + validate options (DataAnnotations + custom validator)
builder.Services.AddOptions<LeaderboardSettings>()
.Bind(builder.Configuration.GetSection("Leaderboard"))
.ValidateDataAnnotations()
.ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<LeaderboardSettings>, LeaderboardSettingsValidator>();

// Add DB Context 
builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Add repository and Service
builder.Services.AddScoped<IPlayerScoreRepository, PlayerScoreRepository>();
builder.Services.AddScoped<ILeaderboardQueries, LeaderboardQueries>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// This is for Health Check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "sql_db",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" }
    );


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ProblemDetails for consistent error responses
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();
app.UseStatusCodePages();


app.UseAuthorization();

app.MapControllers();

app.Run();

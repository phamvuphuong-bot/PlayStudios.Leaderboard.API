using PlayStudios.Leaderboard.Frontend.Components;
using PlayStudios.Leaderboard.Frontend;
using PlayStudios.Leaderboard.Frontend.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();


// Configure HttpClient to call backend API
var apiBase = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    // same-origin by default
    apiBase = builder.Configuration["ASPNETCORE_URLS"] ?? "/";
}


builder.Services.AddHttpClient<LeaderboardApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase!.TrimEnd('/'));
});


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
.AddInteractiveServerRenderMode();


app.Run();
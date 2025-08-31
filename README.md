## 📌 Overview
This repository contains a sample **Leaderboard system** implementation with:
- ASP.NET Core 8 backend Web API (Leaderboard service).
- Blazor frontend WebApp for interacting with the API.
- xUnit automated test project with Moq + EF Core SQLite in-memory.
- Docker containerization and AWS deployment readiness.

## 🏗️ Projects
### 1. PlayStudios.Leaderboard.Api
Backend Web API exposing endpoints to:
- Submit a score (`POST /api/leaderboard/submit`).
- Get leaderboard snapshot (`GET /api/leaderboard?playerId=...`).
- Reset scores (`POST /api/leaderboard/reset`).
- Health checks (`/health/live`, `/health/ready`).

Key Components:
- **Program.cs** – service/middleware configuration.
- **Controllers** – LeaderboardController, HealthController.
- **Infrastructure** – AppDbContext, LeaderboardQueries, PlayerScoreRepository.
- **Domain** – Entities (PlayerScore, RankedPlayer), DTOs (LeaderboardResponse, LeaderboardPlayerDto, SubmitScoreRequest), Config (LeaderboardSettings).
- **Services** – ScoreService (business logic).

### 2. PlayStudios.Leaderboard.FrontEnd
Blazor Web frontend providing UI to:
- Submit scores and view responses.
- Display Top players and Nearby players.
- Reset and seed random data.

Key Components:
- **Pages/Home.razor** – main UI.
- **Services/LeaderboardApiClient** – typed HttpClient wrapper.
- **Models** – request/response DTOs.
- **Program.cs** – configures Blazor server interactivity.

### 3. PlayStudios.Leaderboard.Tests
Automated tests with xUnit:
- **ScoreServiceTests** – verifies Replace/Accumulate modes, Nearby behavior, edge cases.
- **RepositoryTests** – tests PlayerScoreRepository using SQLite in-memory DB.

## 🚀 Run Locally
1. Clone the repo.
2. Build backend API:
   ```bash
   cd PlayStudios.Leaderboard.Api
   dotnet run
   ```
   Swagger available at: `http://localhost:5284/swagger/index.html`

3. Build frontend Blazor app:
   ```bash
   cd PlayStudios.Leaderboard.FrontEnd
   dotnet run
   ```
     
4. Run tests:
   ```bash
   cd PlayStudios.Leaderboard.Tests
   dotnet test
   ```

## 🐳 Docker
Build and run container

## ☁️ Deployment
- Publish container image to AWS ECR.
- Deploy to AWS ECS/Fargate with a service and load balancer.
- Health checks (`/health/live`, `/health/ready`) integrated for orchestration.

## 📖 Documentation
See [Leaderboard_Documentation.docx](Leaderboard_Documentation.docx) for detailed architecture and class-level descriptions.

---
✅ Clean architecture • ✅ Test coverage • ✅ Docker-ready • ✅ Cloud-ready

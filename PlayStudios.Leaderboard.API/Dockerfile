FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5284

ENV ASPNETCORE_URLS=http://+:5284

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["PlayStudios.Leaderboard.API.csproj", "./"]
RUN dotnet restore "PlayStudios.Leaderboard.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PlayStudios.Leaderboard.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "PlayStudios.Leaderboard.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlayStudios.Leaderboard.API.dll"]
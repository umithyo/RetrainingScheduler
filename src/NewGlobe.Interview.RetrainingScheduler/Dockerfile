FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NewGlobe.Interview.RetrainingScheduler/NewGlobe.Interview.RetrainingScheduler.csproj", "NewGlobe.Interview.RetrainingScheduler/"]
RUN dotnet restore "NewGlobe.Interview.RetrainingScheduler/NewGlobe.Interview.RetrainingScheduler.csproj"
COPY . .
WORKDIR "/src/NewGlobe.Interview.RetrainingScheduler"
RUN dotnet build "NewGlobe.Interview.RetrainingScheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewGlobe.Interview.RetrainingScheduler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewGlobe.Interview.RetrainingScheduler.dll"]

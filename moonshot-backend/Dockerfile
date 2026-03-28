# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["moonshot-backend/MoodRadar.sln", "moonshot-backend/"]
COPY ["moonshot-backend/MoodRadar.API/", "moonshot-backend/MoodRadar.API/"]

# Restore dependencies
RUN dotnet restore "moonshot-backend/MoodRadar.sln"

# Build application
RUN dotnet build "moonshot-backend/MoodRadar.sln" -c Release --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "moonshot-backend/MoodRadar.sln" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "MoodRadar.API.dll"]

using DotNetEnv;
using MoodRadar.API.Services;
using MoodRadar.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

// Load .env file BEFORE building the app
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Register Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("PostgreSQL connection string not found in configuration");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register mood update background service (runs every 15 minutes)
builder.Services.AddHostedService<MoodUpdateService>();
// Register Ticketmaster service as Singleton with HttpClient and service provider
// IServiceProvider is used to get a scoped DbContext for saving data
builder.Services.AddHttpClient<TicketmasterService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddScoped<ITicketmasterService>(sp => 
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<TicketmasterService>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var serviceProvider = sp;
    return new TicketmasterService(httpClient, logger, config, serviceProvider);
});

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Weather service as Singleton with HttpClient and service provider
// IServiceProvider is used to get a scoped DbContext for saving data
builder.Services.AddHttpClient<WeatherService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddSingleton<IWeatherService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var serviceProvider = sp;
    var logger = sp.GetRequiredService<ILogger<WeatherService>>();
    return new WeatherService(httpClient, serviceProvider, logger);
});

// Register Venue Scraper service as Singleton with HttpClient and service provider
// Used for daily scraping for Eindhoven events
builder.Services.AddHttpClient<VenueScraperService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });
builder.Services.AddSingleton<IVenueScraperService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<VenueScraperService>>();
    var serviceProvider = sp;
    return new VenueScraperService(httpClient, logger, serviceProvider);
});

// Register other services
builder.Services.AddScoped<MoodPredictionService>();

// Register MoodUpdateService as hosted service
// Injects services directly (no HTTP), consistent with other background operations
builder.Services.AddSingleton<IHostedService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MoodUpdateService>>();
    var serviceProvider = sp;
    var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
    return new MoodUpdateService(logger, serviceProvider, hostEnvironment);
});

// Register Football service with HttpClient
builder.Services.AddScoped<FootballService>();
builder.Services.AddHttpClient("football", client =>
{
    client.BaseAddress = new Uri("https://api.football-data.org/v4/");
    client.DefaultRequestHeaders.Add("X-Auth-Token", builder.Configuration["FootballApi:ApiKey"]);
});

// Define allowed origins based on environment
var allowedOrigins = builder.Environment.IsProduction()
    ? new[] { "" }
    : new[] { "http://localhost:3000" };

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register HolidayService
builder.Services.AddHttpClient<HolidayService>();
builder.Services.AddScoped<HolidayService>();

var app = builder.Build();

// Apply migrations and seed database on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Apply any pending migrations
            Console.WriteLine("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("✓ Migrations applied successfully");

            // Some existing migrations in this repo are no-op; verify that core tables exist.
            if (!await CoreTablesExistAsync(dbContext))
            {
                Console.WriteLine("⚠ Core tables missing after migrations. Rebuilding schema for development...");
                await RebuildSchemaForDevelopmentAsync(dbContext);
                Console.WriteLine("✓ Development schema rebuilt successfully");
            }

            // Seed initial data
            Console.WriteLine("Running database seeder...");
            await DatabaseSeeder.SeedAsync(dbContext);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Error during database setup via migrations: {ex.Message}");

            // Development fallback for broken migration chains:
            // rebuild schema from current model and then seed.
            try
            {
                Console.WriteLine("Attempting development fallback schema rebuild...");
                await RebuildSchemaForDevelopmentAsync(dbContext);
                Console.WriteLine("✓ Fallback schema rebuild succeeded");

                Console.WriteLine("Running database seeder...");
                await DatabaseSeeder.SeedAsync(dbContext);
            }
            catch (Exception fallbackEx)
            {
                Console.Error.WriteLine($"✗ Fallback schema rebuild failed: {fallbackEx.Message}");
                // Don't throw - allow app to start even if DB setup fails
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

// Map controllers
app.MapControllers();

app.Run();

static async Task<bool> CoreTablesExistAsync(ApplicationDbContext dbContext)
{
    var connection = dbContext.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        await connection.OpenAsync();
    }

    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT COUNT(*)
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('Districts', 'Quarters', 'Neighborhoods', 'Events', 'Weathers', 'NeighborhoodSnapshots');";

    var result = await command.ExecuteScalarAsync();
    var tableCount = Convert.ToInt32(result);
    return tableCount >= 6;
}

static async Task RebuildSchemaForDevelopmentAsync(ApplicationDbContext dbContext)
{
    // Remove migration history that may have been created by a broken/no-op migration chain.
    await dbContext.Database.ExecuteSqlRawAsync(@"DROP TABLE IF EXISTS ""__EFMigrationsHistory"";");

    // First attempt without destroying the database.
    await dbContext.Database.EnsureCreatedAsync();

    if (!await CoreTablesExistAsync(dbContext))
    {
        // Last resort for local development: recreate schema from current model.
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    if (!await CoreTablesExistAsync(dbContext))
    {
        throw new InvalidOperationException("Core tables are still missing after schema rebuild.");
    }
}

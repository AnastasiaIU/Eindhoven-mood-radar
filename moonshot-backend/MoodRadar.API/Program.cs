using DotNetEnv;
using MoodRadar.API.Services;
using MoodRadar.API.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
// Register Entity Framework Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? throw new InvalidOperationException("PostgreSQL connection string not found in configuration");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Ticketmaster service as Singleton with HttpClient
// Singleton ensures the in-memory cache persists across requests
builder.Services.AddHttpClient<TicketmasterService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddSingleton<ITicketmasterService>(sp => sp.GetRequiredService<TicketmasterService>());

// Register Weather service as Singleton with HttpClient and service provider
// Singleton ensures the in-memory weather cache persists across requests
// IServiceProvider is used to get a scoped DbContext when saving data
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
// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register other services
builder.Services.AddScoped<FootballService>();
builder.Services.AddHostedService<MoodUpdateService>();

// Define allowed origins based on environment
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddScoped<FootballService>();

// HttpClients
builder.Services.AddHttpClient("football", client =>
{
    client.BaseAddress = new Uri("https://api.football-data.org/v4/");
    client.DefaultRequestHeaders.Add("X-Auth-Token", builder.Configuration["FootballApi:ApiKey"]);
});

builder.Services.AddHttpClient<TicketmasterService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service interfaces
builder.Services.AddSingleton<IWeatherService>(sp =>
    sp.GetRequiredService<WeatherService>());

builder.Services.AddSingleton<ITicketmasterService>(sp =>
    sp.GetRequiredService<TicketmasterService>());

builder.Services.AddHostedService<MoodUpdateService>();

// CORS
var allowedOrigins = builder.Environment.IsProduction()
    ? new[] { "" }
    : new[] { "http://localhost:3000" };

// Configure CORS for frontend access
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register HttpClient with API token
builder.Services.AddHttpClient("football", client =>
{
    client.BaseAddress = new Uri("https://api.football-data.org/v4/");
    client.DefaultRequestHeaders.Add("X-Auth-Token", builder.Configuration["FootballApi:ApiKey"]);
});

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

            // Seed initial data
            Console.WriteLine("Running database seeder...");
            await DatabaseSeeder.SeedAsync(dbContext);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Error during database setup: {ex.Message}");
            // Don't throw - allow app to start even if DB setup fails
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
// Enable Swagger middleware
// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
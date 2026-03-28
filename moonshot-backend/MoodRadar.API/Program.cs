using MoodRadar.API.Services;
using DotNetEnv;

// Load .env file BEFORE building the app
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register mock data service (Phase 1 - development)
builder.Services.AddSingleton<IMockDataService, MockDataService>();

// Register Ticketmaster service as Singleton with HttpClient
// Singleton ensures the in-memory cache persists across requests
builder.Services.AddHttpClient<TicketmasterService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddSingleton<ITicketmasterService>(sp => sp.GetRequiredService<TicketmasterService>());

// Register Weather service as Singleton with HttpClient
// Singleton ensures the in-memory weather cache persists across requests
// Open-Meteo API requires no authentication key
builder.Services.AddHttpClient<WeatherService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.AddSingleton<IWeatherService>(sp => sp.GetRequiredService<WeatherService>());

// Register mood update background service (runs every 15 minutes)
builder.Services.AddHostedService<MoodUpdateService>();

// Define allowed origins based on environment
var allowedOrigins = builder.Environment.IsProduction()
    ? new[] { "" }
    : new[] { "http://localhost:3000" };

// Configure CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

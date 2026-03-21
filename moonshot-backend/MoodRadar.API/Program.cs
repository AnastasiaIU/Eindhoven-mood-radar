using MoodRadar.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register mock data service (Phase 1 - development)
builder.Services.AddSingleton<IMockDataService, MockDataService>();

// Register mood update background service (runs every 15 minutes)
builder.Services.AddHostedService<MoodUpdateService>();

// Configure CORS for frontend access (localhost:3000 for Next.js dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
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

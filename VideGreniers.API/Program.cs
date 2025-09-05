using Microsoft.EntityFrameworkCore;
using Serilog;
using VideGreniers.Infrastructure;
using VideGreniers.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/videgreniers-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Database initialization
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Apply pending migrations
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        
        // Seed the database
        logger.LogInformation("Seeding database...");
        await ApplicationDbContextSeed.SeedAsync(scope.ServiceProvider, logger, isDevelopment: true);
        
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization");
        // Don't throw in development - allow the app to start
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();

// Health check endpoint
app.MapHealthChecks("/health");

// Development-only endpoints
if (app.Environment.IsDevelopment())
{
    // Manual database seeding endpoint
    app.MapPost("/seed", async (IServiceProvider serviceProvider) =>
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Clear existing data
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            logger.LogInformation("Clearing existing data...");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Favorites\", \"Events\", \"Categories\", \"AspNetUsers\", \"AspNetRoles\", \"Users\" RESTART IDENTITY CASCADE");
            
            // Re-seed
            logger.LogInformation("Re-seeding database...");
            await ApplicationDbContextSeed.SeedAsync(scope.ServiceProvider, logger, isDevelopment: true);
            
            return Results.Ok(new { message = "Database re-seeded successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while re-seeding database");
            return Results.Problem("An error occurred while re-seeding the database");
        }
    })
    .WithName("SeedDatabase")
    .WithSummary("Re-seed the database with test data (Development only)");
}

app.MapControllers();

try
{
    Log.Information("Starting VideGreniers API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
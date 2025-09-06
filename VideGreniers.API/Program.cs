using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VideGreniers.API.Extensions;
using VideGreniers.API.Middleware;
using VideGreniers.API.Services;
using VideGreniers.Application;
using VideGreniers.Application.Common.Interfaces;
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// Add custom API services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add service extensions
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddHealthChecks(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRateLimiting();

// Add Application services (includes MediatR)
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

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
        
        // Seed the database (commented out temporarily to avoid seeding issues)
        // logger.LogInformation("Seeding database...");
        // await ApplicationDbContextSeed.SeedAsync(scope.ServiceProvider, logger, isDevelopment: true);
        
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
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vide-Greniers API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ResponseCachingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

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
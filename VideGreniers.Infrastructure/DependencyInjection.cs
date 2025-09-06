using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Interfaces;
using VideGreniers.Infrastructure.Caching;
using VideGreniers.Infrastructure.Identity;
using VideGreniers.Infrastructure.Identity.OAuth;
using VideGreniers.Infrastructure.Persistence;
using VideGreniers.Infrastructure.Persistence.Repositories;
using VideGreniers.Infrastructure.Services;

namespace VideGreniers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));

        // JWT Settings Configuration - temporary manual configuration
        services.Configure<JwtSettings>(options =>
        {
            var jwtSection = configuration.GetSection("JwtSettings");
            options.Secret = jwtSection["Secret"] ?? "development-key-min-32-chars-1234567890";
            options.Issuer = jwtSection["Issuer"] ?? "VideGreniers";
            options.Audience = jwtSection["Audience"] ?? "VideGreniers";
            options.AccessTokenExpirationMinutes = int.TryParse(jwtSection["AccessTokenExpirationMinutes"], out var accessMinutes) ? accessMinutes : 15;
            options.RefreshTokenExpirationDays = int.TryParse(jwtSection["RefreshTokenExpirationDays"], out var refreshDays) ? refreshDays : 7;
        });

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password requirements
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                
                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Generic Repository (required for CQRS handlers)
        services.AddScoped(typeof(Application.Common.Interfaces.IRepository<>), typeof(Repository<>));

        // Specific Repositories (for custom methods not in generic interface)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Authentication Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAuthenticationService, Services.AuthenticationService>();

        // Services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        // Redis (optional, only if connection string is provided)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(redisConnectionString));
            
            // TODO: Replace CacheService with Redis-based implementation when Redis is available
        }

        return services;
    }
}
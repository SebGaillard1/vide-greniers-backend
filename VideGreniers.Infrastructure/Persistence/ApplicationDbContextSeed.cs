using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Domain.ValueObjects;
using VideGreniers.Infrastructure.Identity;

namespace VideGreniers.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        bool isDevelopment = true)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed users and domain data
            await SeedUsersAndDataAsync(context, userManager, logger, isDevelopment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            new ApplicationRole("User", "Standard user who can browse and favorite events"),
            new ApplicationRole("Organizer", "User who can create and manage events"),
            new ApplicationRole("Moderator", "User who can moderate content"),
            new ApplicationRole("Admin", "Administrator with full access")
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", role.Name);
                }
                else
                {
                    logger.LogWarning("Failed to create role: {RoleName}", role.Name);
                }
            }
        }
    }

    private static async Task SeedUsersAndDataAsync(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        ILogger logger,
        bool isDevelopment)
    {
        // Skip seeding if we already have data
        if (await context.DomainUsers.AnyAsync())
        {
            logger.LogInformation("Database already contains data. Skipping seed.");
            return;
        }

        // Create test users
        var users = await CreateTestUsersAsync(context, userManager, logger, isDevelopment);
        
        // Create categories
        var categories = await CreateCategoriesAsync(context, logger);
        
        // Create test events
        await CreateTestEventsAsync(context, users, categories, logger, isDevelopment);
        
        // Create test favorites
        await CreateTestFavoritesAsync(context, users, logger, isDevelopment);
        
        await context.SaveChangesAsync();
        logger.LogInformation("Database seeding completed successfully");
    }

    private static async Task<List<User>> CreateTestUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        bool isDevelopment)
    {
        var users = new List<User>();
        
        if (!isDevelopment) return users;

        // No test users in simplified version
        logger.LogInformation("Skipping test user creation");

        return users;
    }

    private static async Task<List<Category>> CreateCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        var categories = new[]
        {
            Category.Create("Vêtements", "Vêtements adultes et enfants", CategoryType.General, "shirt", "#FF6B6B", 1),
            Category.Create("Livres", "Livres, BD, magazines", CategoryType.General, "book", "#4ECDC4", 2),
            Category.Create("Jouets", "Jouets et jeux pour enfants", CategoryType.General, "toy", "#45B7D1", 3),
            Category.Create("Électroménager", "Appareils électroménagers", CategoryType.General, "appliance", "#96CEB4", 4),
            Category.Create("Mobilier", "Meubles et décoration", CategoryType.General, "furniture", "#FFEAA7", 5),
            Category.Create("Divers", "Objets divers", CategoryType.General, "misc", "#DDA0DD", 6)
        }.Select(result => result.IsError ? null : result.Value)
         .Where(c => c != null)
         .Cast<Category>()
         .ToList();

        context.Categories.AddRange(categories);
        logger.LogInformation("Created {Count} categories", categories.Count);
        
        return categories;
    }

    private static async Task CreateTestEventsAsync(
        ApplicationDbContext context,
        List<User> users,
        List<Category> categories,
        ILogger logger,
        bool isDevelopment)
    {
        // Simplified - no test events
        logger.LogInformation("Skipping test events creation");
        await Task.CompletedTask;
    }

    private static async Task CreateTestFavoritesAsync(
        ApplicationDbContext context,
        List<User> users,
        ILogger logger,
        bool isDevelopment)
    {
        // Simplified - no test favorites
        logger.LogInformation("Skipping test favorites creation");
        await Task.CompletedTask;
    }
}

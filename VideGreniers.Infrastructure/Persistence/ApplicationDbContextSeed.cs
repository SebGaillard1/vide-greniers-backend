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
        
        // Try to get Identity services (optional - they may not be configured yet)
        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
        
        if (userManager == null || roleManager == null)
        {
            logger.LogInformation("Identity services not configured yet. Seeding basic data only.");
        }

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles (only if Identity is configured)
            if (roleManager != null)
            {
                await SeedRolesAsync(roleManager, logger);
            }

            // Seed basic domain data (categories)
            await SeedCategoriesAsync(context, logger);

            // Seed users and events (only if Identity is configured)
            if (userManager != null)
            {
                await SeedUsersAndDataAsync(context, userManager, logger, isDevelopment);
            }
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
        
        // Create categories (only if they don't exist)
        var categories = await EnsureCategoriesAsync(context, logger);
        
        // Create test events
        await CreateTestEventsAsync(context, users, categories, logger, isDevelopment);
        
        // Create test favorites
        await CreateTestFavoritesAsync(context, users, logger, isDevelopment);
        
        await context.SaveChangesAsync();
        logger.LogInformation("Database seeding completed successfully");
    }

    private static Task<List<User>> CreateTestUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        bool isDevelopment)
    {
        var users = new List<User>();
        
        if (!isDevelopment) return Task.FromResult(users);

        // No test users in simplified version
        logger.LogInformation("Skipping test user creation");

        return Task.FromResult(users);
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync())
        {
            logger.LogInformation("Categories already exist, skipping category seeding");
            return;
        }

        var categories = await CreateCategoriesAsync(context, logger);
        await context.SaveChangesAsync();
        logger.LogInformation("Categories seeded successfully: {Count} categories created", categories.Count);
    }

    private static async Task<List<Category>> EnsureCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        // Check if categories already exist
        var existingCategories = await context.Categories.ToListAsync();
        
        if (existingCategories.Count > 0)
        {
            logger.LogInformation("Categories already exist, using existing categories: {Count} found", existingCategories.Count);
            return existingCategories;
        }

        // Create new categories if none exist
        return await CreateCategoriesAsync(context, logger);
    }

    private static Task<List<Category>> CreateCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        var categoryData = new[]
        {
            ("Vêtements", "Vêtements adultes et enfants", "shirt", "#FF6B6B", 1),
            ("Livres", "Livres, BD, magazines", "book", "#4ECDC4", 2),
            ("Jouets", "Jouets et jeux pour enfants", "toy", "#45B7D1", 3),
            ("Électroménager", "Appareils électroménagers", "appliance", "#96CEB4", 4),
            ("Mobilier", "Meubles et décoration", "furniture", "#FFEAA7", 5),
            ("Divers", "Objets divers", "misc", "#DDA0DD", 6)
        };

        var categories = new List<Category>();

        foreach (var (name, description, icon, color, order) in categoryData)
        {
            var categoryResult = Category.Create(name, description, CategoryType.General, icon, color, order);
            if (!categoryResult.IsError)
            {
                categories.Add(categoryResult.Value);
            }
            else
            {
                logger.LogWarning("Failed to create category {Name}: {Errors}", name, string.Join(", ", categoryResult.Errors.Select(e => e.Description)));
            }
        }

        context.Categories.AddRange(categories);
        logger.LogInformation("Created {Count} new categories", categories.Count);
        
        return Task.FromResult(categories);
    }

    private static Task CreateTestEventsAsync(
        ApplicationDbContext context,
        List<User> users,
        List<Category> categories,
        ILogger logger,
        bool isDevelopment)
    {
        // Simplified - no test events
        logger.LogInformation("Skipping test events creation");
        return Task.CompletedTask;
    }

    private static Task CreateTestFavoritesAsync(
        ApplicationDbContext context,
        List<User> users,
        ILogger logger,
        bool isDevelopment)
    {
        // Simplified - no test favorites
        logger.LogInformation("Skipping test favorites creation");
        return Task.CompletedTask;
    }
}

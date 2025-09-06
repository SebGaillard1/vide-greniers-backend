using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Entities;
using VideGreniers.Domain.Enums;
using VideGreniers.Infrastructure.Persistence;

namespace VideGreniers.Infrastructure.Services;

public sealed class UserActivityService : IUserActivityService
{
    private readonly ApplicationDbContext _context;

    public UserActivityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task TrackEventActivityAsync(
        Guid userId,
        UserActivityType activityType,
        Guid eventId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var activity = UserActivity.CreateEventActivity(
            userId,
            activityType,
            eventId,
            ipAddress,
            userAgent);

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task TrackSearchActivityAsync(
        Guid userId,
        string searchTerm,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var activity = UserActivity.CreateSearchActivity(
            userId,
            searchTerm,
            metadata,
            ipAddress,
            userAgent);

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task TrackUserActivityAsync(
        Guid userId,
        UserActivityType activityType,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var activity = UserActivity.CreateUserActivity(
            userId,
            activityType,
            ipAddress,
            userAgent);

        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void TrackActivityAsync(
        Guid userId,
        UserActivityType activityType,
        Guid? eventId = null,
        string? searchTerm = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Fire and forget - track activity in background
        Task.Run(async () =>
        {
            try
            {
                var activity = UserActivity.Create(
                    userId,
                    activityType,
                    eventId,
                    searchTerm,
                    metadata,
                    ipAddress,
                    userAgent);

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silently fail for background tracking
                // In production, you might want to log this
            }
        });
    }
}
using Microsoft.EntityFrameworkCore;
using VideGreniers.Domain.Interfaces;

namespace VideGreniers.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            throw new InvalidOperationException("A concurrency conflict occurred while saving changes.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Log the exception here if needed
            // Transform EF Core exceptions to domain errors if necessary
            throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
        }
    }
}
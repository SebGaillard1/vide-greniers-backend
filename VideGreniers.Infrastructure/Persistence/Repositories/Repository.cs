using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VideGreniers.Application.Common.Interfaces;
using VideGreniers.Domain.Common.Models;
using VideGreniers.Domain.Specifications;

namespace VideGreniers.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation for domain entities
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => !((BaseAuditableEntity)(object)e).IsDeleted) // Apply soft delete filter if applicable
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        // Apply soft delete filter if entity supports it
        if (typeof(T).IsAssignableTo(typeof(BaseAuditableEntity)))
        {
            query = query.Where(e => !((BaseAuditableEntity)(object)e).IsDeleted);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetSingleAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Check if entity supports soft delete
        if (entity is BaseAuditableEntity auditableEntity)
        {
            auditableEntity.SoftDelete(Guid.Empty); // TODO: Get current user ID
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }

        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        // Apply soft delete filter if entity supports it
        if (typeof(T).IsAssignableTo(typeof(BaseAuditableEntity)))
        {
            query = query.Where(e => !((BaseAuditableEntity)(object)e).IsDeleted);
        }

        return await query.AnyAsync(predicate, cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = _dbSet.AsQueryable();

        // Apply criteria from specification
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip)
                        .Take(specification.Take);
        }

        return query;
    }
}
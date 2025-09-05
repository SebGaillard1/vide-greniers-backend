using Microsoft.EntityFrameworkCore;

namespace VideGreniers.Application.Common.Models;

/// <summary>
/// Paginated list wrapper for query results
/// </summary>
/// <typeparam name="T">Type of items in the list</typeparam>
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated list from an IQueryable
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Creates a paginated list from a list and total count
    /// </summary>
    public static PaginatedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
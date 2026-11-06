using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Api.DTOs.Common;

// Generic response wrapper: the items of one page plus the metadata the UI needs
// ("Load more" button => HasNextPage, "Page 2 of 9" => PageNumber / TotalPages).
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 1 : pageSize;
        TotalCount = totalCount;
        // (double) matters: with integer division 11 / 10 would be 1 page instead of 2
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        Items = items;
    }

    // `source` must be an IQueryable with an explicit ORDER BY, so that Skip/Take run in the database
    // and pages are stable. Two statements: COUNT(*) on the full query, then the page itself.
    public static async Task<PagedResult<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Count BEFORE Skip/Take, otherwise TotalCount would never exceed PageSize
        var totalCount = await source.CountAsync(cancellationToken);

        // (page - 1) * pageSize can overflow int and wrap around to page 1, so compute it as long
        long skip = (long)(pageNumber - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            return new PagedResult<T>(new List<T>(), totalCount, pageNumber, pageSize);
        }

        var items = await source
            .Skip((int)skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}

namespace MicroBlog.Api.DTOs.Common;

// Query-string paging parameters shared by every paged endpoint: ?page=2&pageSize=20
// The setters clamp bad values, so a client can never ask for page 0, a negative size,
// or the whole table in one request.
public class PaginationParams
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 10;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    // Alias so that ?pageNumber=2 works too (the name used in the response body)
    public int PageNumber
    {
        get => _page;
        set => Page = value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }
}

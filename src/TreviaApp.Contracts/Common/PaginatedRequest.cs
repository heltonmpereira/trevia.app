namespace TreviaApp.Contracts.Common;

public class PaginatedRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}

namespace TreviaApp.Contracts.Common;

/// <summary>
/// Represents the PaginatedRequest contract.
/// </summary>
public class PaginatedRequest
{
    /// <summary>
    /// Gets or sets Page Index.
    /// </summary>
    public int PageIndex { get; set; } = 0;
    /// <summary>
    /// Gets or sets Page Size.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

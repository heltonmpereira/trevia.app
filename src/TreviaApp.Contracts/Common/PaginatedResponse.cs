namespace TreviaApp.Contracts.Common;

/// <summary>
/// Represents the PaginatedResponse contract.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets Items.
    /// </summary>
    public List<T> Items { get; set; } = new();
    /// <summary>
    /// Gets or sets Total Count.
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Gets or sets Page Index.
    /// </summary>
    public int PageIndex { get; set; }
    /// <summary>
    /// Gets or sets Page Size.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Gets or sets Has Next Page.
    /// </summary>
    public bool HasNextPage { get; set; }
}

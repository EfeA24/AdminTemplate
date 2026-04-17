namespace TrivaWebPage.Abstractions;

public interface IAdminSearchService
{
    Task<IReadOnlyList<AdminSearchHit>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class AdminSearchHit
{
    public required string Kind { get; init; }
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
}

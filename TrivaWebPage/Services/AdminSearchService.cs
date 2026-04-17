using Dapper;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Data.Connection;

namespace TrivaWebPage.Services;

public sealed class AdminSearchService : IAdminSearchService
{
    private const string UnusedPageLabel = "Henüz bir sayfada kullanılmıyor";

    private readonly IDbConnectionFactory _connectionFactory;

    public AdminSearchService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AdminSearchHit>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var term = query.Trim();
        if (term.Length == 0)
        {
            return Array.Empty<AdminSearchHit>();
        }

        var pattern = "%" + EscapeLike(term) + "%";

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string pagesSql = """
            SELECT
                'page' AS Kind,
                p.Id,
                p.Name AS Title,
                p.Slug AS Subtitle
            FROM [Pages] p
            WHERE p.IsDeleted = 0
              AND (
                    p.Name LIKE @Pattern
                    OR p.Slug LIKE @Pattern
                    OR (p.Title IS NOT NULL AND p.Title LIKE @Pattern)
                  );
            """;

        var pageRows = await connection.QueryAsync<SearchRow>(
            new CommandDefinition(pagesSql, new { Pattern = pattern }, cancellationToken: cancellationToken));

        const string actionsSql = """
            WITH Usage AS (
                SELECT DISTINCT bc.ActionDefinitionId AS ActionId, p.Name AS PageName
                FROM [ButtonComponents] bc
                INNER JOIN [PageComponents] pc ON pc.Id = bc.PageComponentId
                INNER JOIN [PageSections] ps ON ps.Id = pc.PageSectionId
                INNER JOIN [Pages] p ON p.Id = ps.PageId
                WHERE bc.ActionDefinitionId IS NOT NULL AND p.IsDeleted = 0
                UNION
                SELECT DISTINCT cb.ActionDefinitionId, p.Name
                FROM [CardButtons] cb
                INNER JOIN [CardComponents] cc ON cc.Id = cb.CardComponentId
                INNER JOIN [PageComponents] pc ON pc.Id = cc.PageComponentId
                INNER JOIN [PageSections] ps ON ps.Id = pc.PageSectionId
                INNER JOIN [Pages] p ON p.Id = ps.PageId
                WHERE cb.ActionDefinitionId IS NOT NULL AND p.IsDeleted = 0
            )
            SELECT
                'action' AS Kind,
                ad.Id,
                ad.Name AS Title,
                NULLIF(STRING_AGG(u.PageName, N', ') WITHIN GROUP (ORDER BY u.PageName), N'') AS Subtitle
            FROM [ActionDefinitions] ad
            LEFT JOIN Usage u ON u.ActionId = ad.Id
            WHERE ad.Name LIKE @Pattern
            GROUP BY ad.Id, ad.Name;
            """;

        var actionRows = await connection.QueryAsync<SearchRow>(
            new CommandDefinition(actionsSql, new { Pattern = pattern }, cancellationToken: cancellationToken));

        var pageHits = pageRows.Select(r => new AdminSearchHit
        {
            Kind = "page",
            Id = r.Id,
            Title = r.Title,
            Subtitle = r.Subtitle
        });

        var actionHits = actionRows.Select(r => new AdminSearchHit
        {
            Kind = "action",
            Id = r.Id,
            Title = r.Title,
            Subtitle = string.IsNullOrWhiteSpace(r.Subtitle) ? UnusedPageLabel : r.Subtitle
        });

        return pageHits
            .Concat(actionHits)
            .OrderBy(h => h.Kind == "page" ? 0 : 1)
            .ThenBy(h => h.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    private sealed class SearchRow
    {
        public string Kind { get; set; } = null!;
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Subtitle { get; set; }
    }
}

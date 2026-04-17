using Dapper;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class PageMediaFileRepository : IPageMediaFile
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PageMediaFileRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<int>> GetMediaFileIdsByPageAsync(int pageId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT [MediaFileId] FROM [PageMediaFiles] WHERE [PageId] = @PageId;";
        var rows = await connection.QueryAsync<int>(new CommandDefinition(sql, new { PageId = pageId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<int>> GetPageIdsForMediaFileAsync(int mediaFileId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT [PageId] FROM [PageMediaFiles] WHERE [MediaFileId] = @MediaFileId;";
        var rows = await connection.QueryAsync<int>(new CommandDefinition(sql, new { MediaFileId = mediaFileId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task ReplaceAssignmentsAsync(int mediaFileId, IReadOnlyCollection<int> pageIds, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        using var tx = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "DELETE FROM [PageMediaFiles] WHERE [MediaFileId] = @MediaFileId;",
                    new { MediaFileId = mediaFileId },
                    transaction: tx,
                    cancellationToken: cancellationToken));

            const string insertSql =
                "INSERT INTO [PageMediaFiles] ([PageId], [MediaFileId]) VALUES (@PageId, @MediaFileId);";

            foreach (var pageId in pageIds.Distinct())
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(insertSql, new { PageId = pageId, MediaFileId = mediaFileId }, transaction: tx, cancellationToken: cancellationToken));
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task RemoveAsync(int pageId, int mediaFileId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM [PageMediaFiles] WHERE [PageId] = @PageId AND [MediaFileId] = @MediaFileId;",
                new { PageId = pageId, MediaFileId = mediaFileId },
                cancellationToken: cancellationToken));
    }

    public async Task DeleteAllForMediaFileAsync(int mediaFileId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM [PageMediaFiles] WHERE [MediaFileId] = @MediaFileId;",
                new { MediaFileId = mediaFileId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> GetAllPageIdsByMediaFileAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT [MediaFileId], [PageId] FROM [PageMediaFiles];";
        var rows = await connection.QueryAsync<(int MediaFileId, int PageId)>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        var dict = rows
            .GroupBy(r => r.MediaFileId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<int>)g.Select(x => x.PageId).Distinct().ToList());
        return dict;
    }
}

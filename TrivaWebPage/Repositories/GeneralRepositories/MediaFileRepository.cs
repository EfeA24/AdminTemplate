using Dapper;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories
{
    public class MediaFileRepository : GenericRepository<MediaFile>, IMediaFile
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MediaFileRepository(
            IDbConnectionFactory connectionFactory,
            string? tableName = null,
            string keyColumnName = "Id")
            : base(
                  connectionFactory,
                  tableName ?? "MediaFiles",
                  keyColumnName)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> HasBlockingReferencesAsync(int mediaFileId, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            const string sql = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [ImageComponents] WHERE [MediaFileId] = @Id
                ) OR EXISTS (
                    SELECT 1 FROM [CardDefinitions] WHERE [PreviewMediaFileId] = @Id
                ) THEN 1 ELSE 0 END;
                """;
            var blocked = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { Id = mediaFileId }, cancellationToken: cancellationToken));
            return blocked == 1;
        }
    }
}

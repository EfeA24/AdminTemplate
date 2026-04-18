using Dapper;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Helpers;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories
{
    public class PageRepository : GenericRepository<Page>, IPage
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PageRepository(
            IDbConnectionFactory connectionFactory,
            string? tableName = null,
            string keyColumnName = "Id")
            : base(
                  connectionFactory,
                  tableName ?? "Pages",
                  keyColumnName)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Page?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var key = SlugNormalizer.Normalize(slug);
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var matches = await GetByConditionAsync(
                "[Slug] = @Slug AND [IsPublished] = 1 AND [IsDeleted] = 0",
                new { Slug = key },
                cancellationToken);

            return matches.Count > 0 ? matches[0] : null;
        }

        public async Task<Page?> GetDefaultPublishedHomePageAsync(CancellationToken cancellationToken = default)
        {
            var slugHomeMatches = await GetByConditionAsync(
                "[Slug] = @Slug AND [IsPublished] = 1 AND [IsDeleted] = 0 ORDER BY [DisplayOrder], [Id]",
                new { Slug = SlugNormalizer.Normalize("home") },
                cancellationToken);

            if (slugHomeMatches.Count > 0)
            {
                return slugHomeMatches[0];
            }

            var homeCandidates = await GetByConditionAsync(
                "[IsHomePage] = 1 AND [IsPublished] = 1 AND [IsDeleted] = 0 ORDER BY [DisplayOrder], [Id]",
                null,
                cancellationToken);

            if (homeCandidates.Count > 0)
            {
                return homeCandidates[0];
            }

            var anyPublished = await GetByConditionAsync(
                "[IsPublished] = 1 AND [IsDeleted] = 0 ORDER BY [DisplayOrder], [Id]",
                null,
                cancellationToken);

            if (anyPublished.Count > 0)
            {
                return anyPublished[0];
            }

            return null;
        }

        public async Task<bool> SlugExistsForAnotherPageAsync(
            string slug,
            int? exceptPageId,
            CancellationToken cancellationToken = default)
        {
            var key = SlugNormalizer.Normalize(slug);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            const string sql = """
                               SELECT CAST(CASE WHEN EXISTS(
                                   SELECT 1 FROM [Pages]
                                   WHERE [Slug] = @Slug AND [IsDeleted] = 0
                                     AND (@ExceptId IS NULL OR [Id] <> @ExceptId)
                               ) THEN 1 ELSE 0 END AS bit);
                               """;

            return await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { Slug = key, ExceptId = exceptPageId }, cancellationToken: cancellationToken));
        }

        public async Task ClearHomePageFlagExceptAsync(int? exceptPageId, CancellationToken cancellationToken = default)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            const string sql = """
                               UPDATE [Pages]
                               SET [IsHomePage] = 0
                               WHERE [IsHomePage] = 1
                                 AND (@ExceptId IS NULL OR [Id] <> @ExceptId);
                               """;

            await connection.ExecuteAsync(new CommandDefinition(sql, new { ExceptId = exceptPageId }, cancellationToken: cancellationToken));
        }
    }
}

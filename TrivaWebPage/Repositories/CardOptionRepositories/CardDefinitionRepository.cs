using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.CardOptions;

namespace TrivaWebPage.Repositories.CardOptionRepositories;

public class CardDefinitionRepository : ICardDefinition
{
    private const string SelectList = """
        [Id], [Name], [Code], [PreviewImageUrl], [Description], [IsSystem], [CreatedDate], [UpdatedDate]
        """;

    private readonly AppDbContext _dbContext;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _tableName = "CardDefinitions";
    private readonly string _keyColumnName = "Id";

    public CardDefinitionRepository(AppDbContext dbContext, IDbConnectionFactory connectionFactory)
    {
        _dbContext = dbContext;
        _connectionFactory = connectionFactory;
    }

    public async Task<CardDefinition?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT {SelectList} FROM [{_tableName}] WHERE [{_keyColumnName}] = @Id;";
        return await connection.QuerySingleOrDefaultAsync<CardDefinition>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<CardDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT {SelectList} FROM [{_tableName}] ORDER BY [Name];";
        var rows = await connection.QueryAsync<CardDefinition>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<int> CreateAsync(CardDefinition entity, CancellationToken cancellationToken = default)
    {
        _dbContext.CardDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(CardDefinition entity, CancellationToken cancellationToken = default)
    {
        var tracked = await _dbContext.CardDefinitions
            .FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
        if (tracked is null)
        {
            return false;
        }

        tracked.Name = entity.Name;
        tracked.Code = entity.Code;
        tracked.PreviewImageUrl = entity.PreviewImageUrl;
        tracked.Description = entity.Description;
        tracked.IsSystem = entity.IsSystem;
        tracked.UpdatedDate = entity.UpdatedDate;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tracked = await _dbContext.CardDefinitions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (tracked is null)
        {
            return false;
        }

        if (tracked.IsSystem)
        {
            return false;
        }

        _dbContext.CardDefinitions.Remove(tracked);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"""
                   SELECT CAST(
                       CASE WHEN EXISTS(
                           SELECT 1 FROM [{_tableName}] WHERE [{_keyColumnName}] = @Id
                       ) THEN 1 ELSE 0 END
                   AS bit);
                   """;
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT COUNT(1) FROM [{_tableName}];";
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<CardDefinition>> GetByConditionAsync(
        string whereClause,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
        {
            throw new ArgumentException("whereClause cannot be empty.", nameof(whereClause));
        }

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT {SelectList} FROM [{_tableName}] WHERE {whereClause};";
        var rows = await connection.QueryAsync<CardDefinition>(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<(IReadOnlyList<CardDefinition> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? whereClause = null,
        object? parameters = null,
        string? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var offset = (pageNumber - 1) * pageSize;
        var effectiveOrderBy = string.IsNullOrWhiteSpace(orderBy) ? $"[{_keyColumnName}]" : orderBy;

        var selectSql = new StringBuilder($"SELECT {SelectList} FROM [{_tableName}]");
        var countSql = new StringBuilder($"SELECT COUNT(1) FROM [{_tableName}]");

        if (!string.IsNullOrWhiteSpace(whereClause))
        {
            selectSql.Append(" WHERE ").Append(whereClause);
            countSql.Append(" WHERE ").Append(whereClause);
        }

        selectSql.Append($" ORDER BY {effectiveOrderBy} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");
        countSql.Append(';');

        var dynamicParams = new DynamicParameters(parameters);
        dynamicParams.Add("Offset", offset);
        dynamicParams.Add("PageSize", pageSize);

        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            $"{selectSql}\n{countSql}",
            dynamicParams,
            cancellationToken: cancellationToken));

        var items = (await multi.ReadAsync<CardDefinition>()).AsList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return (items, totalCount);
    }
}

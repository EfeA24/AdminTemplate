using System.Collections;
using System.Reflection;
using System.Text;
using Dapper;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Data.Connection;

namespace TrivaWebPage.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private static readonly PropertyInfo[] AllProperties;
    private static readonly PropertyInfo KeyProperty;
    private static readonly PropertyInfo[] NonKeyProperties;

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _tableName;
    private readonly string _keyColumnName;

    /// <summary>
    /// EF entities often carry navigation properties; Dapper INSERT/UPDATE must only use scalar columns.
    /// </summary>
    private static bool MapsToSqlColumn(PropertyInfo p)
    {
        var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

        if (t.IsEnum)
        {
            return true;
        }

        if (t == typeof(string) || t == typeof(byte[]) || t == typeof(Guid) ||
            t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan) ||
            t == typeof(decimal))
        {
            return true;
        }

        if (t.IsPrimitive)
        {
            return true;
        }

        // Skip navigations and collections (e.g. ICollection&lt;T&gt; on MediaFile, Page, …)
        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string))
        {
            return false;
        }

        return false;
    }

    static GenericRepository()
    {
        AllProperties = typeof(TEntity)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        KeyProperty = AllProperties.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Entity '{typeof(TEntity).Name}' must have an int Id property.");

        if (KeyProperty.PropertyType != typeof(int))
        {
            throw new InvalidOperationException($"Entity '{typeof(TEntity).Name}' Id property must be int.");
        }

        NonKeyProperties = AllProperties
            .Where(p => p != KeyProperty && MapsToSqlColumn(p))
            .ToArray();
    }

    public GenericRepository(IDbConnectionFactory connectionFactory, string? tableName = null, string keyColumnName = "Id")
    {
        _connectionFactory = connectionFactory;
        _tableName = tableName ?? typeof(TEntity).Name;
        _keyColumnName = keyColumnName;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT * FROM [{_tableName}] WHERE [{_keyColumnName}] = @Id;";

        return await connection.QuerySingleOrDefaultAsync<TEntity>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT * FROM [{_tableName}];";
        var rows = await connection.QueryAsync<TEntity>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (NonKeyProperties.Length == 0)
        {
            throw new InvalidOperationException($"Entity '{typeof(TEntity).Name}' must contain at least one non-key property.");
        }

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var columns = string.Join(", ", NonKeyProperties.Select(p => $"[{p.Name}]"));
        var values = string.Join(", ", NonKeyProperties.Select(p => $"@{p.Name}"));

        var sql = new StringBuilder()
            .Append($"INSERT INTO [{_tableName}] ({columns}) ")
            .Append($"OUTPUT INSERTED.[{_keyColumnName}] ")
            .Append($"VALUES ({values});")
            .ToString();

        var parameters = new DynamicParameters();
        foreach (var prop in NonKeyProperties)
        {
            parameters.Add(prop.Name, prop.GetValue(entity));
        }

        var newId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));

        KeyProperty.SetValue(entity, newId);
        return newId;
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (NonKeyProperties.Length == 0)
        {
            return false;
        }

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var setClause = string.Join(", ", NonKeyProperties.Select(p => $"[{p.Name}] = @{p.Name}"));
        var sql = $"UPDATE [{_tableName}] SET {setClause} WHERE [{_keyColumnName}] = @Id;";

        var parameters = new DynamicParameters();
        foreach (var prop in NonKeyProperties)
        {
            parameters.Add(prop.Name, prop.GetValue(entity));
        }

        var idValue = KeyProperty.GetValue(entity);
        parameters.Add("Id", idValue);

        var affected = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"DELETE FROM [{_tableName}] WHERE [{_keyColumnName}] = @Id;";

        var affected = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));

        return affected > 0;
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

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<TEntity>> GetByConditionAsync(
        string whereClause,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(whereClause))
        {
            throw new ArgumentException("whereClause cannot be empty. Pass only condition body (without WHERE).", nameof(whereClause));
        }

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var sql = $"SELECT * FROM [{_tableName}] WHERE {whereClause};";

        var rows = await connection.QueryAsync<TEntity>(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));

        return rows.AsList();
    }

    public async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
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

        var selectSql = new StringBuilder($"SELECT * FROM [{_tableName}]");
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

        var items = (await multi.ReadAsync<TEntity>()).AsList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return (items, totalCount);
    }
}

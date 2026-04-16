using System.Data;

namespace TrivaWebPage.Data.Connection;

public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection.
    /// The caller is responsible for disposing the returned connection.
    /// </summary>
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}


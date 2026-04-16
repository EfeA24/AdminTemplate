using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TrivaWebPage.Data.Connection;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        // Convention: use "DefaultConnection" from configuration.
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        // SqlConnection does not support true async open, but this keeps the async shape for callers.
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return Task.FromResult<IDbConnection>(connection);
    }
}


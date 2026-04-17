using Dapper;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models;

namespace TrivaWebPage.Repositories;

public class UserRepository : GenericRepository<User>, IUser
{
    private readonly IDbConnectionFactory _dbFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "Users", "Id")
    {
        _dbFactory = connectionFactory;
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = "SELECT * FROM [Users] WHERE [UserName] = @UserName;";
        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserName = userName }, cancellationToken: cancellationToken));
    }
}

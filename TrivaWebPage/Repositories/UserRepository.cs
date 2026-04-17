using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models;

namespace TrivaWebPage.Repositories
{
    public class UserRepository : GenericRepository<User>, Abstractions.IUser
    {
        public UserRepository(IDbConnectionFactory connectionFactory, string? tableName = null, string keyColumnName = "Id") : base(connectionFactory, tableName, keyColumnName)
        {
        }
    }
}

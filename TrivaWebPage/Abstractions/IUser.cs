using TrivaWebPage.Models;

namespace TrivaWebPage.Abstractions;

public interface IUser : IGenericRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
}


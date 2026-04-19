using Microsoft.AspNetCore.Identity;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Models;

namespace TrivaWebPage.Services;

/// <summary>
/// Ensures the default admin account exists and accepts the configured demo password.
/// </summary>
public sealed class AdminUserSeedService
{
    public const string DefaultUserName = "admin";
    public const string DefaultPassword = "1234";

    private readonly IUser _users;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminUserSeedService(IUser users, IPasswordHasher<User> passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _users.GetByUserNameAsync(DefaultUserName, cancellationToken);
        if (existing is null)
        {
            var user = new User
            {
                UserName = DefaultUserName,
                PasswordHash = string.Empty
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, DefaultPassword);
            await _users.CreateAsync(user, cancellationToken);
            return;
        }

        var verify = _passwordHasher.VerifyHashedPassword(existing, existing.PasswordHash, DefaultPassword);
        if (verify == PasswordVerificationResult.Success)
        {
            return;
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            existing.PasswordHash = _passwordHasher.HashPassword(existing, DefaultPassword);
            await _users.UpdateAsync(existing, cancellationToken);
            return;
        }

        existing.PasswordHash = _passwordHasher.HashPassword(existing, DefaultPassword);
        await _users.UpdateAsync(existing, cancellationToken);
    }
}

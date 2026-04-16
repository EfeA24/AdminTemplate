using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Repositories;

namespace TrivaWebPage.DependencyInjection;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection factory
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

        // Generic repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}


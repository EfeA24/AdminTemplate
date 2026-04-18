using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Repositories;
using TrivaWebPage.Repositories.CardOptionRepositories;
using TrivaWebPage.Repositories.ContentRepositories;
using TrivaWebPage.Repositories.GeneralRepositories;
using TrivaWebPage.Services;

namespace TrivaWebPage.DependencyInjection;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection factory
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IAdminSearchService, AdminSearchService>();
        services.AddScoped<IUser, UserRepository>();

        // Generic repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Card option repositories
        services.AddScoped<IActionDefinition, ActionDefinitionRepository>();
        services.AddScoped<ICardButton, CardButtonRepository>();
        services.AddScoped<ICardDefinition, CardDefinitionRepository>();
        services.AddScoped<ICardFieldDefinition, CardFieldDefinitionRepository>();
        services.AddScoped<ICardFieldValue, CardFieldValueRepository>();

        // Content repositories
        services.AddScoped<IButtonComponent, ButtonComponentRepository>();
        services.AddScoped<ICardComponent, CardComponentRepository>();
        services.AddScoped<IImageComponent, ImageComponentRepository>();
        services.AddScoped<ITextComponent, TextComponentRepository>();

        // General repositories
        services.AddScoped<IMediaFile, MediaFileRepository>();
        services.AddScoped<IPageMediaFile, PageMediaFileRepository>();
        services.AddScoped<IPage, PageRepository>();
        services.AddScoped<IPageTemplate, PageTemplateRepository>();
        services.AddScoped<IPageTemplatePage, PageTemplatePageRepository>();
        services.AddScoped<IColorPalette, ColorPaletteRepository>();
        services.AddScoped<IPageComponent, PageComponentRepository>();
        services.AddScoped<IPageSection, PageSectionRepository>();
        services.AddScoped<IPageTextBuilderRepository, PageTextBuilderRepository>();
        services.AddScoped<IPageCardBuilderRepository, PageCardBuilderRepository>();
        services.AddScoped<CuratorTemplateSeedService>();
        services.AddScoped<CardDefinitionSeedService>();

        return services;
    }
}


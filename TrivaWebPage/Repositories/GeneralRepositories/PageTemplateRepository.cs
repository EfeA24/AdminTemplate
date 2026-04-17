using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class PageTemplateRepository : GenericRepository<PageTemplate>, IPageTemplate
{
    public PageTemplateRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "PageTemplates")
    {
    }
}

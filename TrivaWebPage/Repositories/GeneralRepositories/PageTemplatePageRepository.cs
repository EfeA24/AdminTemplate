using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class PageTemplatePageRepository : GenericRepository<PageTemplatePage>, IPageTemplatePage
{
    public PageTemplatePageRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "PageTemplatePages")
    {
    }
}

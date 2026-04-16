using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Repositories.ContentRepositories
{
    public class TextComponentRepository : GenericRepository<TextComponent>, ITextComponent
    {
        public TextComponentRepository(
            IDbConnectionFactory connectionFactory,
            string? tableName = null,
            string keyColumnName = "Id")
            : base(
                  connectionFactory,
                  tableName,
                  keyColumnName)
        {
        }
    }
}

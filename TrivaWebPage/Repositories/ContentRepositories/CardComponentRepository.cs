using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Repositories.ContentRepositories
{
    public class CardComponentRepository : GenericRepository<CardComponent>, ICardComponent
    {
        public CardComponentRepository(
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

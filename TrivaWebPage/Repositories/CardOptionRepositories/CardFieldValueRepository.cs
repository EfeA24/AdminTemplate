using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.CardOptions;

namespace TrivaWebPage.Repositories.CardOptionRepositories
{
    public class CardFieldValueRepository : GenericRepository<CardFieldValue>, ICardFieldValue
    {
        public CardFieldValueRepository(
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

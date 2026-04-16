using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.CardOptions;

namespace TrivaWebPage.Repositories.CardOptionRepositories
{
    public class CardFieldDefinitionRepository : GenericRepository<CardFieldDefinition>, ICardFieldDefinition
    {
        public CardFieldDefinitionRepository(
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

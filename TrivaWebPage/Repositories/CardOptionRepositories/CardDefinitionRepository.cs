using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.CardOptions;

namespace TrivaWebPage.Repositories.CardOptionRepositories
{
    public class CardDefinitionRepository : GenericRepository<CardDefinition>, ICardDefinition
    {
        public CardDefinitionRepository(
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

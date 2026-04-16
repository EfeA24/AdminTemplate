using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories
{
    public class MediaFileRepository : GenericRepository<MediaFile>, IMediaFile
    {
        public MediaFileRepository(
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

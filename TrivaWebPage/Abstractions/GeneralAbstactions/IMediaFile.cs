using TrivaWebPage.Models.General;

namespace TrivaWebPage.Abstractions.GeneralAbstactions
{
    public interface IMediaFile : IGenericRepository<MediaFile>
    {
        Task<bool> HasBlockingReferencesAsync(int mediaFileId, CancellationToken cancellationToken = default);
    }
}

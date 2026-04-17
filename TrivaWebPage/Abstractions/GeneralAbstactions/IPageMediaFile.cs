namespace TrivaWebPage.Abstractions.GeneralAbstactions;

public interface IPageMediaFile
{
    Task<IReadOnlyList<int>> GetMediaFileIdsByPageAsync(int pageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetPageIdsForMediaFileAsync(int mediaFileId, CancellationToken cancellationToken = default);

    Task ReplaceAssignmentsAsync(int mediaFileId, IReadOnlyCollection<int> pageIds, CancellationToken cancellationToken = default);

    Task RemoveAsync(int pageId, int mediaFileId, CancellationToken cancellationToken = default);

    Task DeleteAllForMediaFileAsync(int mediaFileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, IReadOnlyList<int>>> GetAllPageIdsByMediaFileAsync(CancellationToken cancellationToken = default);
}

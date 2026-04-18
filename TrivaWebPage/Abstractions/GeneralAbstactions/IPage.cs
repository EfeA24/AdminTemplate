using TrivaWebPage.Models.General;

namespace TrivaWebPage.Abstractions.GeneralAbstactions
{
    public interface IPage : IGenericRepository<Page>
    {
        Task<Page?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task<Page?> GetDefaultPublishedHomePageAsync(CancellationToken cancellationToken = default);

        Task<bool> SlugExistsForAnotherPageAsync(string slug, int? exceptPageId, CancellationToken cancellationToken = default);

        Task ClearHomePageFlagExceptAsync(int? exceptPageId, CancellationToken cancellationToken = default);
    }
}

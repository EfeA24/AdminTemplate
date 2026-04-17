using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Abstractions.GeneralAbstactions;

public interface IPageCardBuilderRepository
{
    Task<CardsEditorPageData?> GetPageEditorDataAsync(int pageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardButtonPresetViewModel>> GetButtonPresetsAsync(CancellationToken cancellationToken = default);

    Task SavePageCardsAsync(int pageId, IReadOnlyList<CardBuilderSaveItemInputModel> items, CancellationToken cancellationToken = default);

    Task UpdateCardComponentMediaAsync(int cardComponentId, int? mediaFileId, CancellationToken cancellationToken = default);
}

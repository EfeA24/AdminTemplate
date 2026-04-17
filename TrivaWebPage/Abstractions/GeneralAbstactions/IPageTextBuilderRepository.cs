using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Abstractions.GeneralAbstactions;

public interface IPageTextBuilderRepository
{
    Task<TextsEditorPageData?> GetPageEditorDataAsync(int pageId, CancellationToken cancellationToken = default);

    Task SavePageTextBoxesAsync(int pageId, IReadOnlyList<TextBoxSaveItemInputModel> items, CancellationToken cancellationToken = default);
}

using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.ViewModels.Public;

public class PublicSitePageComposedViewModel
{
    public string Slug { get; init; } = string.Empty;
    public string? BrowserTitle { get; init; }
    public int PageWidth { get; init; }
    public int PageHeight { get; init; }
    public string TemplateHtml { get; init; } = string.Empty;
    public IReadOnlyList<TextBoxEditorItemViewModel> TextItems { get; init; } = Array.Empty<TextBoxEditorItemViewModel>();
    public IReadOnlyList<CardEditorItemViewModel> Cards { get; init; } = Array.Empty<CardEditorItemViewModel>();
}

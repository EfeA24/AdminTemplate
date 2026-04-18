namespace TrivaWebPage.ViewModels.Public;

public class PublicSitePageFullDocumentViewModel
{
    public string? BrowserTitle { get; init; }

    /// <summary>Tam HTML belge (iframe srcdoc için view tarafında encode edilir).</summary>
    public string DocumentHtml { get; init; } = string.Empty;
}

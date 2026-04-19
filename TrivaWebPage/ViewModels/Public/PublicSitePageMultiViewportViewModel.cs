namespace TrivaWebPage.ViewModels.Public;

/// <summary>Public shell with three full-document iframes; CSS shows one per viewport width.</summary>
public class PublicSitePageMultiViewportViewModel
{
    public string? BrowserTitle { get; init; }

    public string DesktopHtml { get; init; } = string.Empty;
    public string TabletHtml { get; init; } = string.Empty;
    public string PhoneHtml { get; init; } = string.Empty;
}

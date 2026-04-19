namespace TrivaWebPage.Helpers;

/// <summary>Builds a full HTML document from template inner HTML (fragment or document) for canvas/public bootstrap.</summary>
public static class PageTemplateHtmlBootstrapper
{
    public static string EnsureFullHtmlDocument(string? templateInnerOrDocument)
    {
        var raw = templateInnerOrDocument ?? string.Empty;
        if (PublicPageHtml.LooksLikeFullHtmlDocument(raw))
        {
            return raw.Trim();
        }

        var body = raw.Trim();
        if (string.IsNullOrEmpty(body))
        {
            return "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\" /><title></title></head><body></body></html>";
        }

        return "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\" /><title></title></head><body>" + body + "</body></html>";
    }
}

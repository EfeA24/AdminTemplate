namespace TrivaWebPage.Helpers;

public static class PublicPageHtml
{
    /// <summary>
    /// Tam HTML belgeleri (DOCTYPE veya html kökü) ham text/html olarak döndürülür; aksi halde bileşik renderer kullanılır.
    /// </summary>
    public static bool LooksLikeFullHtmlDocument(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        var trimmed = html.TrimStart();
        if (trimmed.Length > 0 && trimmed[0] == '\uFEFF')
        {
            trimmed = trimmed.TrimStart('\uFEFF').TrimStart();
        }
        return trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }
}

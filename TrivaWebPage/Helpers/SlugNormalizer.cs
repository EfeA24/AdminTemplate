namespace TrivaWebPage.Helpers;

public static class SlugNormalizer
{
    public static string Normalize(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return string.Empty;
        }

        return slug.Trim().ToLowerInvariant();
    }
}

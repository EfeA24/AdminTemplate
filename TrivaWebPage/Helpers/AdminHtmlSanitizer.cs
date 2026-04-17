using System.Text.RegularExpressions;

namespace TrivaWebPage.Helpers;

public static partial class AdminHtmlSanitizer
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var html = input.Trim();
        html = ScriptTagRegex().Replace(html, string.Empty);
        html = EventHandlerAttributeRegex().Replace(html, string.Empty);
        return html;
    }

    [GeneratedRegex("<script\\b[^<]*(?:(?!<\\/script>)<[^<]*)*<\\/script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex("\\son[a-z]+\\s*=\\s*(['\"]).*?\\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex EventHandlerAttributeRegex();
}

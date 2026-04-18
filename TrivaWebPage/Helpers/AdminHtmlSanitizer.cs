using System.Text.RegularExpressions;

namespace TrivaWebPage.Helpers;

/// <summary>
/// Şablon kaydında XSS riskini azaltır. Güvenilir harici stil/yardımcı script adreslerine izin verilir;
/// satır içi script ve olay öznitelikleri kaldırılır.
/// </summary>
public static partial class AdminHtmlSanitizer
{
    private static readonly string[] AllowedScriptSrcPrefixes =
    {
        "https://cdn.tailwindcss.com/",
        "https://cdn.jsdelivr.net/npm/tailwindcss",
    };

    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var html = input.Trim();
        var placeholders = new Dictionary<string, string>(StringComparer.Ordinal);
        var i = 0;

        html = ScriptTagRegex().Replace(html, match =>
        {
            if (TryGetAllowlistedExternalScript(match.Value, out var preserved))
            {
                var key = $"<!--triva-ph-{i}-->";
                placeholders[key] = preserved;
                i++;
                return key;
            }

            return string.Empty;
        });

        html = ScriptTagRegex().Replace(html, string.Empty);
        html = EventHandlerAttributeRegex().Replace(html, string.Empty);

        foreach (var kv in placeholders)
        {
            html = html.Replace(kv.Key, kv.Value, StringComparison.Ordinal);
        }

        return html;
    }

    /// <summary>
    /// Yalnızca https src + bilinen CDN öneki ve satır içi kod içermeyen script etiketleri korunur.
    /// </summary>
    private static bool TryGetAllowlistedExternalScript(string scriptTag, out string preserved)
    {
        preserved = scriptTag;
        if (string.IsNullOrWhiteSpace(scriptTag))
        {
            return false;
        }

        var srcMatch = ScriptSrcAttributeRegex().Match(scriptTag);
        if (!srcMatch.Success)
        {
            return false;
        }

        var src = srcMatch.Groups[2].Value.Trim();
        if (src.Length == 0 || !src.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var allowed = false;
        foreach (var prefix in AllowedScriptSrcPrefixes)
        {
            if (src.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                allowed = true;
                break;
            }
        }

        if (!allowed)
        {
            return false;
        }

        var inner = ScriptInnerContentRegex().Match(scriptTag);
        if (inner.Success && inner.Groups[1].Value.Trim().Length > 0)
        {
            return false;
        }

        return true;
    }

    [GeneratedRegex("<script\\b[^<]*(?:(?!<\\/script>)<[^<]*)*<\\/script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex("\\ssrc\\s*=\\s*([\"'])([^\"']+)\\1", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptSrcAttributeRegex();

    [GeneratedRegex("<script\\b[^>]*>([\\s\\S]*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptInnerContentRegex();

    [GeneratedRegex("\\son[a-z]+\\s*=\\s*(['\"]).*?\\1", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex EventHandlerAttributeRegex();
}

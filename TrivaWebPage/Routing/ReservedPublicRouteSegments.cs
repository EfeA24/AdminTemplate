namespace TrivaWebPage.Routing;

/// <summary>
/// Single-segment public URLs map to <see cref="Controllers.SitePageController"/>.
/// These values are reserved so they still resolve to MVC controllers and static paths.
/// </summary>
public static class ReservedPublicRouteSegments
{
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        // Conventional MVC controller route segments (no "Controller" suffix)
        "account",
        "adminsearch",
        "actiondefinitions",
        "buttoncomponents",
        "cardbuttons",
        "cardcomponents",
        "carddefinitions",
        "cardfielddefinitions",
        "cardfieldvalues",
        "cards",
        "help",
        "home",
        "imagecomponents",
        "images",
        "mediafiles",
        "pagecomponents",
        "pageedit",
        "pagesections",
        "pagetemplates",
        "pages",
        "sitepage",
        "templatecanvas",
        "textcomponents",
        "users",
        "websitesettings",
        // Former public URL prefix (bookmarks)
        "sayfa",
    };

    public static bool IsReserved(string slug) => Reserved.Contains(slug);
}

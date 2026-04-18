using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TrivaWebPage.Routing;

/// <summary>
/// Allows only one URL segment that is not reserved for MVC and has no '.' (avoids file-like paths).
/// </summary>
public sealed class PublicPageSlugRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var raw) || raw is not string slug || slug.Length == 0)
        {
            return false;
        }

        if (slug.Contains('.'))
        {
            return false;
        }

        return !ReservedPublicRouteSegments.IsReserved(slug);
    }
}

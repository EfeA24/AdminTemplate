using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions;

namespace TrivaWebPage.Controllers;

public class AdminSearchController : Controller
{
    private readonly IAdminSearchService _adminSearchService;

    public AdminSearchController(IAdminSearchService adminSearchService)
    {
        _adminSearchService = adminSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> Quick(string? q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(Array.Empty<object>());
        }

        var hits = await _adminSearchService.SearchAsync(q, cancellationToken);

        var payload = hits.Select(h => new
        {
            kind = h.Kind,
            id = h.Id,
            title = h.Title,
            subtitle = h.Subtitle,
            url = h.Kind == "page"
                ? Url.Action("Details", "Pages", new { id = h.Id })
                : Url.Action("Details", "ActionDefinitions", new { id = h.Id })
        }).ToList();

        return Json(payload);
    }
}

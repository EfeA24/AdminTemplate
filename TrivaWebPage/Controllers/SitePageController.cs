using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Services;

namespace TrivaWebPage.Controllers;

[AllowAnonymous]
public class SitePageController : Controller
{
    private readonly IPage _pageRepository;
    private readonly PublicSitePageRenderer _renderer;

    public SitePageController(IPage pageRepository, PublicSitePageRenderer renderer)
    {
        _pageRepository = pageRepository;
        _renderer = renderer;
    }

    [HttpGet]
    public async Task<IActionResult> BySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetPublishedBySlugAsync(slug, cancellationToken);
        if (page is null)
        {
            return NotFound();
        }

        return await _renderer.RenderAsync(this, page, cancellationToken);
    }
}

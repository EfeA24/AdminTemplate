using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models;
using TrivaWebPage.Services;

namespace TrivaWebPage.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPage _pageRepository;
        private readonly PublicSitePageRenderer _publicSitePageRenderer;

        public HomeController(IPage pageRepository, PublicSitePageRenderer publicSitePageRenderer)
        {
            _pageRepository = pageRepository;
            _publicSitePageRenderer = publicSitePageRenderer;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var page = await _pageRepository.GetDefaultPublishedHomePageAsync(cancellationToken);
            if (page is not null)
            {
                return await _publicSitePageRenderer.RenderAsync(this, page, cancellationToken);
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

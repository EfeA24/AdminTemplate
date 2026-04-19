using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Helpers;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Public;

namespace TrivaWebPage.Services;

public class PublicSitePageRenderer
{
    private readonly IPageTextBuilderRepository _textBuilderRepository;
    private readonly IPageCardBuilderRepository _cardBuilderRepository;
    private readonly IPageTemplatePage _pageTemplatePageRepository;

    public PublicSitePageRenderer(
        IPageTextBuilderRepository textBuilderRepository,
        IPageCardBuilderRepository cardBuilderRepository,
        IPageTemplatePage pageTemplatePageRepository)
    {
        _textBuilderRepository = textBuilderRepository;
        _cardBuilderRepository = cardBuilderRepository;
        _pageTemplatePageRepository = pageTemplatePageRepository;
    }

    public async Task<IActionResult> RenderAsync(Controller controller, Page page, CancellationToken cancellationToken)
    {
        if (PublicPageHtml.LooksLikeFullHtmlDocument(page.RenderedHtmlOverride))
        {
            var hasTablet = !string.IsNullOrWhiteSpace(page.RenderedHtmlOverrideTablet);
            var hasPhone = !string.IsNullOrWhiteSpace(page.RenderedHtmlOverridePhone);
            if (hasTablet || hasPhone)
            {
                var desktop = page.RenderedHtmlOverride!;
                var tablet = ResolveBreakpointHtml(page.RenderedHtmlOverrideTablet, desktop);
                var phone = ResolveBreakpointHtml(page.RenderedHtmlOverridePhone, desktop);

                var multiVm = new PublicSitePageMultiViewportViewModel
                {
                    BrowserTitle = string.IsNullOrWhiteSpace(page.Title) ? page.Name : page.Title,
                    DesktopHtml = desktop,
                    TabletHtml = tablet,
                    PhoneHtml = phone
                };

                return controller.View("~/Views/SitePage/MultiViewportFull.cshtml", multiVm);
            }

            return controller.Content(page.RenderedHtmlOverride!, "text/html; charset=utf-8");
        }

        var editor = await _textBuilderRepository.GetPageEditorDataAsync(page.Id, cancellationToken);
        var cardsPage = await _cardBuilderRepository.GetPageEditorDataAsync(page.Id, cancellationToken);

        var pageWidth = editor?.PageWidth ?? page.Width;
        var pageHeight = editor?.PageHeight ?? page.Height;

        var templateHtml = editor?.TemplateHtml;
        if (string.IsNullOrWhiteSpace(templateHtml))
        {
            templateHtml = page.RenderedHtmlOverride;
        }

        if (string.IsNullOrWhiteSpace(templateHtml) && page.PageTemplatePageId is int templatePageId)
        {
            var templatePage = await _pageTemplatePageRepository.GetByIdAsync(templatePageId, cancellationToken);
            templateHtml = templatePage?.HtmlContent;
        }

        if (string.IsNullOrWhiteSpace(templateHtml))
        {
            templateHtml =
                "<!DOCTYPE html><html><head><meta charset=\"utf-8\" /><title></title></head><body></body></html>";
        }

        var vm = new PublicSitePageComposedViewModel
        {
            Slug = page.Slug,
            BrowserTitle = string.IsNullOrWhiteSpace(page.Title) ? page.Name : page.Title,
            PageWidth = pageWidth,
            PageHeight = pageHeight,
            TemplateHtml = templateHtml,
            TextItems = editor?.Items ?? [],
            Cards = cardsPage?.Items ?? []
        };

        return controller.View("~/Views/SitePage/Composed.cshtml", vm);
    }

    private static string ResolveBreakpointHtml(string? candidate, string desktopFallback)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return desktopFallback;
        }

        return PublicPageHtml.LooksLikeFullHtmlDocument(candidate) ? candidate : desktopFallback;
    }
}

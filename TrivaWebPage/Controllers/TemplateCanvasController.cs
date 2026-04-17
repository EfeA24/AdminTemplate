using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Helpers;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class TemplateCanvasController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageTemplatePage _templatePageRepository;
    private readonly IColorPalette _colorPaletteRepository;

    public TemplateCanvasController(
        IPage pageRepository,
        IPageTemplatePage templatePageRepository,
        IColorPalette colorPaletteRepository)
    {
        _pageRepository = pageRepository;
        _templatePageRepository = templatePageRepository;
        _colorPaletteRepository = colorPaletteRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? pageId, CancellationToken cancellationToken)
    {
        var pages = (await _pageRepository.GetAllAsync(cancellationToken))
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PageTabItem
            {
                Id = x.Id,
                Name = string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title!
            })
            .ToList();

        int? activePageId = null;
        if (pageId is > 0 && pages.Any(x => x.Id == pageId.Value))
        {
            activePageId = pageId.Value;
        }
        else if (pages.Count > 0)
        {
            activePageId = pages[0].Id;
        }

        TemplateCanvasPageData? activePage = null;
        if (activePageId is int selectedPageId)
        {
            var page = await _pageRepository.GetByIdAsync(selectedPageId, cancellationToken);
            if (page is not null && !page.IsDeleted)
            {
                string? templatePageName = null;
                string? templateHtml = null;
                if (page.PageTemplatePageId is int templatePageId)
                {
                    var templatePage = await _templatePageRepository.GetByIdAsync(templatePageId, cancellationToken);
                    if (templatePage is not null)
                    {
                        templatePageName = templatePage.Name;
                        templateHtml = templatePage.HtmlContent;
                    }
                }

                activePage = new TemplateCanvasPageData
                {
                    PageId = page.Id,
                    PageName = string.IsNullOrWhiteSpace(page.Title) ? page.Name : page.Title!,
                    PageWidth = page.Width,
                    PageHeight = page.Height,
                    TemplatePageName = templatePageName,
                    HtmlContent = string.IsNullOrWhiteSpace(page.RenderedHtmlOverride) ? templateHtml : page.RenderedHtmlOverride,
                    Palette = await ResolvePaletteAsync(page.ColorPaletteId, cancellationToken)
                };
            }
        }

        return View(new TemplateCanvasViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            ActivePage = activePage
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TemplateCanvasSaveInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["TemplateCanvasError"] = "Geçersiz kaydetme isteği.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        var page = await _pageRepository.GetByIdAsync(model.PageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            TempData["TemplateCanvasError"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var sanitized = AdminHtmlSanitizer.Sanitize(model.HtmlContent);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            TempData["TemplateCanvasError"] = "Kaydedilecek HTML içeriği boş olamaz.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        page.RenderedHtmlOverride = sanitized;
        page.UpdatedDate = DateTime.UtcNow;
        await _pageRepository.UpdateAsync(page, cancellationToken);
        TempData["TemplateCanvasMessage"] = "Şablon düzeni kaydedildi.";
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(int pageId, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            TempData["TemplateCanvasError"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        page.RenderedHtmlOverride = null;
        page.UpdatedDate = DateTime.UtcNow;
        await _pageRepository.UpdateAsync(page, cancellationToken);
        TempData["TemplateCanvasMessage"] = "Sayfa şablonu varsayılan haline döndürüldü.";
        return RedirectToAction(nameof(Index), new { pageId });
    }

    private async Task<TemplateCanvasPaletteData?> ResolvePaletteAsync(int? colorPaletteId, CancellationToken cancellationToken)
    {
        if (colorPaletteId is not int id || id <= 0)
        {
            return null;
        }

        var palette = await _colorPaletteRepository.GetByIdAsync(id, cancellationToken);
        if (palette is null)
        {
            return null;
        }

        return new TemplateCanvasPaletteData
        {
            Id = palette.Id,
            Name = palette.Name,
            PrimaryHex = palette.PrimaryHex,
            SecondaryHex = palette.SecondaryHex,
            MutedHex = palette.MutedHex,
            AccentHex = palette.AccentHex
        };
    }
}

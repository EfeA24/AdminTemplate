using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Helpers;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class TemplateCanvasController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageTemplatePage _templatePageRepository;
    private readonly IColorPalette _colorPaletteRepository;
    private readonly IPageMediaFile _pageMediaFile;
    private readonly IMediaFile _mediaFile;
    private readonly IWebHostEnvironment _environment;
    private readonly ICardDefinition _cardDefinitionRepository;

    public TemplateCanvasController(
        IPage pageRepository,
        IPageTemplatePage templatePageRepository,
        IColorPalette colorPaletteRepository,
        IPageMediaFile pageMediaFile,
        IMediaFile mediaFile,
        IWebHostEnvironment environment,
        ICardDefinition cardDefinitionRepository)
    {
        _pageRepository = pageRepository;
        _templatePageRepository = templatePageRepository;
        _colorPaletteRepository = colorPaletteRepository;
        _pageMediaFile = pageMediaFile;
        _mediaFile = mediaFile;
        _environment = environment;
        _cardDefinitionRepository = cardDefinitionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ListCardDefinitions(CancellationToken cancellationToken)
    {
        var list = (await _cardDefinitionRepository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name, x.PreviewImageUrl })
            .ToList();
        return Json(list);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? pageId, CancellationToken cancellationToken)
    {
        var availablePalettes = (await _colorPaletteRepository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.Name)
            .Select(x => new TemplateCanvasPaletteData
            {
                Id = x.Id,
                Name = x.Name,
                PrimaryHex = x.PrimaryHex,
                SecondaryHex = x.SecondaryHex,
                MutedHex = x.MutedHex,
                AccentHex = x.AccentHex
            })
            .ToList();

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
        IReadOnlyList<PageEditMediaItemViewModel> pageMedia = Array.Empty<PageEditMediaItemViewModel>();
        var allMediaEntities = (await _mediaFile.GetAllAsync(cancellationToken))
            .OrderByDescending(m => m.UploadedDate)
            .ToList();
        var libraryMedia = allMediaEntities
            .Select(m => new PageEditMediaItemViewModel
            {
                MediaFileId = m.Id,
                FilePath = m.FilePath,
                DisplayName = m.OriginalFileName
            })
            .ToList();

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

                var desktopHtml = string.IsNullOrWhiteSpace(page.RenderedHtmlOverride) ? templateHtml : page.RenderedHtmlOverride;
                var tabletHtml = string.IsNullOrWhiteSpace(page.RenderedHtmlOverrideTablet) ? templateHtml : page.RenderedHtmlOverrideTablet;
                var phoneHtml = string.IsNullOrWhiteSpace(page.RenderedHtmlOverridePhone) ? templateHtml : page.RenderedHtmlOverridePhone;

                activePage = new TemplateCanvasPageData
                {
                    PageId = page.Id,
                    PageName = string.IsNullOrWhiteSpace(page.Title) ? page.Name : page.Title!,
                    PageWidth = page.Width,
                    PageHeight = page.Height,
                    TemplatePageName = templatePageName,
                    HtmlContent = desktopHtml,
                    HtmlContentTablet = tabletHtml,
                    HtmlContentPhone = phoneHtml,
                    Palette = await ResolvePaletteAsync(page.ColorPaletteId, cancellationToken)
                };

                pageMedia = await AdminPageMediaUpload.LoadPageMediaAsync(selectedPageId, _pageMediaFile, _mediaFile, cancellationToken);
            }
        }

        return View(new TemplateCanvasViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            AvailablePalettes = availablePalettes,
            ActivePage = activePage,
            PageMedia = pageMedia,
            LibraryMedia = libraryMedia
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = AdminPageMediaUpload.MaxUploadBytes)]
    [RequestSizeLimit(AdminPageMediaUpload.MaxUploadBytes)]
    public async Task<IActionResult> UploadMedia(int pageId, IFormFile? file, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            return BadRequest(new { error = "Sayfa bulunamadı." });
        }

        var outcome = await AdminPageMediaUpload.TryUploadAndLinkAsync(
            file,
            _environment,
            _mediaFile,
            pageId,
            _pageMediaFile,
            cancellationToken);

        if (!outcome.Success)
        {
            return BadRequest(new { error = outcome.ErrorMessage });
        }

        return Json(new
        {
            mediaFileId = outcome.MediaFileId,
            filePath = outcome.FilePath,
            displayName = outcome.DisplayName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = AdminPageMediaUpload.MaxUploadBytes)]
    [RequestSizeLimit(AdminPageMediaUpload.MaxUploadBytes)]
    public async Task<IActionResult> UploadLibraryMedia(IFormFile? file, CancellationToken cancellationToken)
    {
        var outcome = await AdminPageMediaUpload.TryCreateMediaFileAsync(
            file,
            _environment,
            _mediaFile,
            cancellationToken);

        if (!outcome.Success)
        {
            return BadRequest(new { error = outcome.ErrorMessage });
        }

        return Json(new
        {
            mediaFileId = outcome.MediaFileId,
            filePath = outcome.FilePath,
            displayName = outcome.DisplayName
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

        var desktopSanitized = AdminHtmlSanitizer.Sanitize(model.HtmlContent);
        if (string.IsNullOrWhiteSpace(desktopSanitized))
        {
            TempData["TemplateCanvasError"] = "Kaydedilecek HTML içeriği boş olamaz.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        var tabletSource = string.IsNullOrWhiteSpace(model.HtmlContentTablet) ? model.HtmlContent : model.HtmlContentTablet;
        var phoneSource = string.IsNullOrWhiteSpace(model.HtmlContentPhone) ? model.HtmlContent : model.HtmlContentPhone;
        var tabletSanitized = AdminHtmlSanitizer.Sanitize(tabletSource ?? string.Empty);
        var phoneSanitized = AdminHtmlSanitizer.Sanitize(phoneSource ?? string.Empty);

        page.RenderedHtmlOverride = desktopSanitized;
        page.RenderedHtmlOverrideTablet = string.IsNullOrWhiteSpace(tabletSanitized) ? desktopSanitized : tabletSanitized;
        page.RenderedHtmlOverridePhone = string.IsNullOrWhiteSpace(phoneSanitized) ? desktopSanitized : phoneSanitized;
        page.ColorPaletteId = model.ColorPaletteId > 0 ? model.ColorPaletteId : null;
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
        page.RenderedHtmlOverrideTablet = null;
        page.RenderedHtmlOverridePhone = null;
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

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Helpers;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageEditController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageTextBuilderRepository _textBuilderRepository;
    private readonly IPageCardBuilderRepository _cardBuilderRepository;
    private readonly IPageMediaFile _pageMediaFile;
    private readonly IMediaFile _mediaFile;
    private readonly IWebHostEnvironment _environment;

    public PageEditController(
        IPage pageRepository,
        IPageTextBuilderRepository textBuilderRepository,
        IPageCardBuilderRepository cardBuilderRepository,
        IPageMediaFile pageMediaFile,
        IMediaFile mediaFile,
        IWebHostEnvironment environment)
    {
        _pageRepository = pageRepository;
        _textBuilderRepository = textBuilderRepository;
        _cardBuilderRepository = cardBuilderRepository;
        _pageMediaFile = pageMediaFile;
        _mediaFile = mediaFile;
        _environment = environment;
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

        PageEditCanvasPageData? activePage = null;
        IReadOnlyList<PageEditMediaItemViewModel> pageMedia = Array.Empty<PageEditMediaItemViewModel>();
        IReadOnlyList<PageEditCardPreviewItemViewModel> cards = Array.Empty<PageEditCardPreviewItemViewModel>();

        if (activePageId is int validPageId)
        {
            activePage = await _textBuilderRepository.GetPageEditorDataAsync(validPageId, cancellationToken);

            pageMedia = await AdminPageMediaUpload.LoadPageMediaAsync(validPageId, _pageMediaFile, _mediaFile, cancellationToken);

            var cardData = await _cardBuilderRepository.GetPageEditorDataAsync(validPageId, cancellationToken);
            if (cardData is not null)
            {
                cards = cardData.Items
                    .Select(c => new PageEditCardPreviewItemViewModel
                    {
                        ComponentId = c.ComponentId,
                        CardComponentId = c.CardComponentId,
                        Title = c.Title,
                        MediaFileId = c.MediaFileId,
                        MediaFilePath = c.MediaFilePath,
                        ShowImage = c.ShowImage,
                        HtmlFragment = c.HtmlFragment
                    })
                    .ToList();
            }
        }

        return View(new PageEditIndexViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            ActivePage = activePage,
            PageMedia = pageMedia,
            Cards = cards
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PageEditSaveInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["PageEditError"] = "Geçersiz kaydetme isteği.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        List<TextBoxSaveItemInputModel>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<TextBoxSaveItemInputModel>>(
                model.PayloadJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException)
        {
            TempData["PageEditError"] = "Metin kutuları okunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items is null)
        {
            TempData["PageEditError"] = "Kaydedilecek veri bulunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items.Count > 300)
        {
            TempData["PageEditError"] = "Aynı anda en fazla 300 metin kutusu kaydedebilirsiniz.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        await _textBuilderRepository.SavePageTextBoxesAsync(model.PageId, items, cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.RenderedHtmlOverride))
        {
            var sanitized = AdminHtmlSanitizer.Sanitize(model.RenderedHtmlOverride);
            if (!string.IsNullOrWhiteSpace(sanitized))
            {
                var page = await _pageRepository.GetByIdAsync(model.PageId, cancellationToken);
                if (page is not null && !page.IsDeleted)
                {
                    page.RenderedHtmlOverride = sanitized;
                    page.UpdatedDate = DateTime.UtcNow;
                    await _pageRepository.UpdateAsync(page, cancellationToken);
                }
            }
        }

        TempData["PageEditMessage"] = "Sayfa düzeni kaydedildi.";
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
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
            TempData["PageEditError"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
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
            TempData["PageEditError"] = outcome.ErrorMessage;
            return RedirectToAction(nameof(Index), new { pageId });
        }

        TempData["PageEditMessage"] = "Resim yüklendi ve sayfaya bağlandı.";
        return RedirectToAction(nameof(Index), new { pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePageMedia(int pageId, int mediaFileId, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            return NotFound();
        }

        await _pageMediaFile.RemoveAsync(pageId, mediaFileId, cancellationToken);
        TempData["PageEditMessage"] = "Görsel sayfa bağlantısı kaldırıldı.";
        return RedirectToAction(nameof(Index), new { pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCardMedia(int pageId, int cardComponentId, int? mediaFileId, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            return NotFound();
        }

        if (mediaFileId is int mid)
        {
            var onPage = await _pageMediaFile.GetMediaFileIdsByPageAsync(pageId, cancellationToken);
            if (!onPage.Contains(mid))
            {
                TempData["PageEditError"] = "Bu görsel bu sayfaya yüklenmemiş.";
                return RedirectToAction(nameof(Index), new { pageId });
            }
        }

        await _cardBuilderRepository.UpdateCardComponentMediaAsync(cardComponentId, mediaFileId, cancellationToken);
        TempData["PageEditMessage"] = "Kart görseli güncellendi.";
        return RedirectToAction(nameof(Index), new { pageId });
    }
}

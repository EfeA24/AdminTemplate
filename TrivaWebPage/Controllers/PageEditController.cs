using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageEditController : Controller
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

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

            var mediaIds = await _pageMediaFile.GetMediaFileIdsByPageAsync(validPageId, cancellationToken);
            var mediaList = new List<PageEditMediaItemViewModel>();
            foreach (var mid in mediaIds)
            {
                var m = await _mediaFile.GetByIdAsync(mid, cancellationToken);
                if (m is null) continue;
                mediaList.Add(new PageEditMediaItemViewModel
                {
                    MediaFileId = m.Id,
                    FilePath = m.FilePath,
                    DisplayName = m.OriginalFileName
                });
            }

            pageMedia = mediaList;

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
        TempData["PageEditMessage"] = "Sayfa düzeni kaydedildi.";
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> UploadMedia(int pageId, IFormFile? file, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            TempData["PageEditError"] = "Sayfa bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        if (file is null || file.Length == 0)
        {
            TempData["PageEditError"] = "Lütfen bir dosya seçin.";
            return RedirectToAction(nameof(Index), new { pageId });
        }

        if (file.Length > MaxUploadBytes)
        {
            TempData["PageEditError"] = "Dosya boyutu en fazla 10 MB olabilir.";
            return RedirectToAction(nameof(Index), new { pageId });
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            TempData["PageEditError"] = "Yalnızca JPG, PNG, GIF veya WEBP yükleyebilirsiniz.";
            return RedirectToAction(nameof(Index), new { pageId });
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            TempData["PageEditError"] = "Geçersiz dosya türü.";
            return RedirectToAction(nameof(Index), new { pageId });
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var relativeDir = Path.Combine("uploads", "media");
        var physicalDir = Path.Combine(webRoot, relativeDir);
        Directory.CreateDirectory(physicalDir);

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalDir, storedName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var webPath = "/" + relativeDir.Replace(Path.DirectorySeparatorChar, '/') + "/" + storedName;

        var entity = new MediaFile
        {
            FileName = storedName,
            OriginalFileName = Path.GetFileName(file.FileName),
            FilePath = webPath,
            AltText = null,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileExtension = ext.TrimStart('.'),
            Width = null,
            Height = null,
            UploadedDate = DateTime.UtcNow
        };

        var mediaId = await _mediaFile.CreateAsync(entity, cancellationToken);
        await _pageMediaFile.EnsureLinkedAsync(pageId, mediaId, cancellationToken);
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

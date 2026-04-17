using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class ImagesController : Controller
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

    private readonly IMediaFile _mediaFile;
    private readonly IPage _page;
    private readonly IPageMediaFile _pageMediaFile;
    private readonly IWebHostEnvironment _environment;

    public ImagesController(
        IMediaFile mediaFile,
        IPage page,
        IPageMediaFile pageMediaFile,
        IWebHostEnvironment environment)
    {
        _mediaFile = mediaFile;
        _page = page;
        _pageMediaFile = pageMediaFile;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? pageId, CancellationToken cancellationToken)
    {
        var allMedia = (await _mediaFile.GetAllAsync(cancellationToken)).OrderByDescending(m => m.UploadedDate).ToList();
        var pageTabs = (await _page.GetAllAsync(cancellationToken))
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .Select(p => new PageTabItem { Id = p.Id, Name = string.IsNullOrWhiteSpace(p.Title) ? p.Name : p.Title! })
            .ToList();

        var activeTab = ImagesGalleryViewModel.TabAll;
        int? activePageId = null;
        if (pageId is > 0 && pageTabs.Any(t => t.Id == pageId))
        {
            activeTab = "page";
            activePageId = pageId;
        }

        List<MediaFile> items;
        if (activePageId is int pid)
        {
            var onPage = await _pageMediaFile.GetMediaFileIdsByPageAsync(pid, cancellationToken);
            var set = onPage.ToHashSet();
            items = allMedia.Where(m => set.Contains(m.Id)).ToList();
        }
        else
        {
            items = allMedia;
        }

        var assignments = await _pageMediaFile.GetAllPageIdsByMediaFileAsync(cancellationToken);

        var vm = new ImagesGalleryViewModel
        {
            ActiveTab = activeTab,
            ActivePageId = activePageId,
            Pages = pageTabs,
            Items = items.Select(m => new MediaFileCardItem
            {
                Id = m.Id,
                DisplayName = m.OriginalFileName,
                FilePath = m.FilePath
            }).ToList(),
            AssignmentsByMediaFileId = assignments
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ImagesError"] = "Lütfen bir dosya seçin.";
            return RedirectToAction(nameof(Index));
        }

        if (file.Length > MaxUploadBytes)
        {
            TempData["ImagesError"] = "Dosya boyutu en fazla 10 MB olabilir.";
            return RedirectToAction(nameof(Index));
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            TempData["ImagesError"] = "Yalnızca JPG, PNG, GIF veya WEBP yükleyebilirsiniz.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            TempData["ImagesError"] = "Geçersiz dosya türü.";
            return RedirectToAction(nameof(Index));
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

        await _mediaFile.CreateAsync(entity, cancellationToken);
        TempData["ImagesMessage"] = "Resim yüklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(ImageAssignInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ImagesError"] = "Geçersiz atama isteği.";
            return RedirectToAction(nameof(Index));
        }

        var media = await _mediaFile.GetByIdAsync(model.MediaFileId, cancellationToken);
        if (media is null)
        {
            TempData["ImagesError"] = "Resim bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var pageIds = model.PageIds ?? Array.Empty<int>();
        var validPageIds = (await _page.GetAllAsync(cancellationToken))
            .Where(p => !p.IsDeleted)
            .Select(p => p.Id)
            .ToHashSet();

        var filtered = pageIds.Where(validPageIds.Contains).Distinct().ToList();
        await _pageMediaFile.ReplaceAssignmentsAsync(model.MediaFileId, filtered, cancellationToken);
        TempData["ImagesMessage"] = "Sayfa atamaları güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(int pageId, int mediaFileId, CancellationToken cancellationToken)
    {
        var page = await _page.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            return NotFound();
        }

        await _pageMediaFile.RemoveAsync(pageId, mediaFileId, cancellationToken);
        TempData["ImagesMessage"] = "Resim sayfadan kaldırıldı.";
        return RedirectToAction(nameof(Index), new { pageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var media = await _mediaFile.GetByIdAsync(id, cancellationToken);
        if (media is null)
        {
            TempData["ImagesError"] = "Resim bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        if (await _mediaFile.HasBlockingReferencesAsync(id, cancellationToken))
        {
            TempData["ImagesError"] =
                "Bu resim bir sayfa bileşeninde veya kart tanımında kullanıldığı için silinemez. Önce oradan kaldırın.";
            return RedirectToAction(nameof(Index));
        }

        await _pageMediaFile.DeleteAllForMediaFileAsync(id, cancellationToken);

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var relative = media.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, relative));
        var rootFull = Path.GetFullPath(webRoot);
        if (fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) &&
            System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        await _mediaFile.DeleteAsync(id, cancellationToken);
        TempData["ImagesMessage"] = "Resim silindi.";
        return RedirectToAction(nameof(Index));
    }
}

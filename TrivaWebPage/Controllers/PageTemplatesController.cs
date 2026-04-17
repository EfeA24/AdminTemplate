using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageTemplatesController : Controller
{
    private const long MaxZipBytes = 25 * 1024 * 1024;
    private const long MaxPreviewBytes = 5 * 1024 * 1024;

    private readonly IPageTemplate _templateRepository;
    private readonly IPageTemplatePage _templatePageRepository;
    private readonly IWebHostEnvironment _environment;

    public PageTemplatesController(
        IPageTemplate templateRepository,
        IPageTemplatePage templatePageRepository,
        IWebHostEnvironment environment)
    {
        _templateRepository = templateRepository;
        _templatePageRepository = templatePageRepository;
        _environment = environment;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        var templates = (await _templateRepository.GetAllAsync(cancellationToken))
            .OrderBy(x => x.Name)
            .ToList();
        var templatePages = await _templatePageRepository.GetAllAsync(cancellationToken);
        ViewBag.TemplatePagesLookup = templatePages
            .GroupBy(x => x.PageTemplateId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToList());
        return View("~/Views/PageTemplates/Index.cshtml", templates);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        var pages = await _templatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
            new { TemplateId = id },
            cancellationToken);

        ViewBag.TemplatePages = pages;
        return View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Şablonlar";
        return View("Form", new PageTemplateEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxZipBytes + MaxPreviewBytes)]
    [RequestSizeLimit(MaxZipBytes + MaxPreviewBytes)]
    public async Task<IActionResult> Create(PageTemplateEditViewModel model, IFormFile? zipArchive, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        await BuildPagesFromInputsAsync(model, zipArchive, cancellationToken);

        if (!ValidateTemplatePages(model))
            return View("Form", model);

        if (!ModelState.IsValid) return View("Form", model);

        var code = await GenerateUniqueCodeAsync(model.Name, excludeId: null, cancellationToken);
        var template = new PageTemplate
        {
            Name = model.Name.Trim(),
            Code = code,
            Description = null,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var templateId = await _templateRepository.CreateAsync(template, cancellationToken);
        await FinalizeAssetFolderAsync(model, templateId, cancellationToken);
        await SaveTemplatePagesAsync(templateId, model.Pages, cancellationToken);
        await ApplyPreviewImageAsync(templateId, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        var pages = await _templatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
            new { TemplateId = id },
            cancellationToken);

        var ordered = pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToList();
        var master = ordered.Count == 1 ? ordered[0].HtmlContent : "";

        ViewBag.DisplayName = "Şablonlar";
        return View("Form", new PageTemplateEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            MasterHtml = master,
            Pages = ordered.Select(p => new PageTemplatePageLineViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DisplayOrder = p.DisplayOrder,
                HtmlContent = p.HtmlContent,
                PreviewImagePath = p.PreviewImagePath
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxZipBytes + MaxPreviewBytes)]
    [RequestSizeLimit(MaxZipBytes + MaxPreviewBytes)]
    public async Task<IActionResult> Edit(int id, PageTemplateEditViewModel model, IFormFile? zipArchive, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        if (id != model.Id) return BadRequest();

        await BuildPagesFromInputsAsync(model, zipArchive, cancellationToken);

        if (!ValidateTemplatePages(model))
            return View("Form", model);

        if (!ModelState.IsValid) return View("Form", model);

        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = model.Name.Trim();
        entity.Code = await GenerateUniqueCodeAsync(model.Name, excludeId: id, cancellationToken);
        await _templateRepository.UpdateAsync(entity, cancellationToken);

        await FinalizeAssetFolderAsync(model, id, cancellationToken);
        var preserveIds = model.Pages.Count > 0 && model.Pages.All(p => p.Id > 0);
        if (preserveIds && (zipArchive is null || zipArchive.Length == 0))
        {
            await SyncTemplatePagesAsync(id, model.Pages, cancellationToken);
        }
        else
        {
            await ReplaceTemplatePagesAsync(id, model.Pages, cancellationToken);
        }

        await ApplyPreviewImageAsync(id, cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await DeleteAssetFolderAsync(id, cancellationToken);
        await _templateRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task BuildPagesFromInputsAsync(
        PageTemplateEditViewModel model,
        IFormFile? zipArchive,
        CancellationToken cancellationToken)
    {
        model.Pages.Clear();

        if (zipArchive is not null && zipArchive.Length > 0)
        {
            if (zipArchive.Length > MaxZipBytes)
            {
                ModelState.AddModelError(string.Empty, "ZIP dosyası en fazla 25 MB olabilir.");
                return;
            }

            var ext = Path.GetExtension(zipArchive.FileName);
            if (!string.Equals(ext, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Yalnızca .zip yükleyebilirsiniz.");
                return;
            }

            await using var ms = new MemoryStream();
            await zipArchive.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var storageKey = $"temp-{Guid.NewGuid():N}";
            var webBase = "/uploads/template-bundles/" + storageKey + "/";

            ExtractZipAssetsAndCollectHtml(ms, storageKey, out var htmlPages);
            if (htmlPages.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "ZIP içinde .html dosyası bulunamadı.");
                return;
            }

            var order = 0;
            foreach (var (relPath, html) in htmlPages)
            {
                var name = Path.GetFileNameWithoutExtension(relPath);
                if (string.IsNullOrWhiteSpace(name)) name = $"Sayfa-{order + 1}";
                var withBase = EnsureBaseHref(html, webBase);
                model.Pages.Add(new PageTemplatePageLineViewModel
                {
                    Id = 0,
                    Name = name,
                    DisplayOrder = order++,
                    HtmlContent = withBase
                });
            }

            ViewBag.PendingAssetStorageKey = storageKey;
            return;
        }

        if (model.Id > 0)
        {
            if (string.IsNullOrWhiteSpace(model.MasterHtml))
            {
                var existing = await _templatePageRepository.GetByConditionAsync(
                    "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
                    new { TemplateId = model.Id },
                    cancellationToken);
                if (existing.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Şablon içi sayfa bulunamadı.");
                    return;
                }

                foreach (var p in existing)
                {
                    model.Pages.Add(new PageTemplatePageLineViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayOrder = p.DisplayOrder,
                        HtmlContent = p.HtmlContent,
                        PreviewImagePath = p.PreviewImagePath
                    });
                }

                return;
            }

            model.Pages.Add(new PageTemplatePageLineViewModel
            {
                Id = 0,
                Name = "Sayfa 1",
                DisplayOrder = 0,
                HtmlContent = model.MasterHtml.Trim()
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(model.MasterHtml))
        {
            ModelState.AddModelError(nameof(model.MasterHtml), "HTML girin veya ZIP yükleyin.");
            return;
        }

        model.Pages.Add(new PageTemplatePageLineViewModel
        {
            Id = 0,
            Name = "Sayfa 1",
            DisplayOrder = 0,
            HtmlContent = model.MasterHtml.Trim()
        });
    }

    private async Task FinalizeAssetFolderAsync(PageTemplateEditViewModel model, int templateId, CancellationToken cancellationToken)
    {
        if (ViewBag.PendingAssetStorageKey is not string key || string.IsNullOrEmpty(key))
        {
            return;
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var bundlesRoot = Path.Combine(webRoot, "uploads", "template-bundles");
        var tempPath = Path.Combine(bundlesRoot, key);
        var finalPath = Path.Combine(bundlesRoot, templateId.ToString());

        if (Directory.Exists(finalPath))
        {
            Directory.Delete(finalPath, recursive: true);
        }

        if (Directory.Exists(tempPath))
        {
            Directory.Move(tempPath, finalPath);
        }

        var finalWebBase = "/uploads/template-bundles/" + templateId + "/";
        var tempWebBase = "/uploads/template-bundles/" + key + "/";

        foreach (var page in model.Pages)
        {
            if (string.IsNullOrEmpty(page.HtmlContent)) continue;
            page.HtmlContent = page.HtmlContent.Replace(tempWebBase, finalWebBase, StringComparison.Ordinal);
        }

        await Task.CompletedTask;
    }

    private Task DeleteAssetFolderAsync(int templateId, CancellationToken cancellationToken)
    {
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var finalPath = Path.Combine(webRoot, "uploads", "template-bundles", templateId.ToString());
        if (Directory.Exists(finalPath))
        {
            Directory.Delete(finalPath, recursive: true);
        }

        return Task.CompletedTask;
    }

    private void ExtractZipAssetsAndCollectHtml(Stream zipStream, string storageKey, out List<(string RelativePath, string Html)> htmlPages)
    {
        htmlPages = new List<(string, string)>();
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var targetRoot = Path.Combine(webRoot, "uploads", "template-bundles", storageKey);
        Directory.CreateDirectory(targetRoot);

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in archive.Entries.OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase))
        {
            var full = entry.FullName.Replace('\\', '/');
            if (string.IsNullOrEmpty(entry.Name) && !full.EndsWith('/')) continue;
            if (full.Contains("..", StringComparison.Ordinal) || full.StartsWith("/", StringComparison.Ordinal)) continue;
            if (full.StartsWith("__MACOSX/", StringComparison.OrdinalIgnoreCase)) continue;

            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            var destPath = Path.Combine(targetRoot, full.Replace('/', Path.DirectorySeparatorChar));
            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (entry.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                var html = reader.ReadToEnd();
                htmlPages.Add((full, html));
            }
            else
            {
                using var entryStream = entry.Open();
                using var outFs = System.IO.File.Create(destPath);
                entryStream.CopyTo(outFs);
            }
        }
    }

    private static string EnsureBaseHref(string html, string baseHref)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;
        if (html.Contains("<base ", StringComparison.OrdinalIgnoreCase)) return html;
        var tag = $"<base href=\"{baseHref}\">";
        var headIdx = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
        if (headIdx >= 0)
        {
            var close = html.IndexOf('>', headIdx);
            if (close > headIdx)
            {
                return html.Insert(close + 1, tag);
            }
        }

        return "<head>" + tag + "</head>\n" + html;
    }

    private static string Slugify(string name)
    {
        var lower = name.Trim().ToLowerInvariant();
        var s = Regex.Replace(lower, @"[^a-z0-9]+", "-", RegexOptions.CultureInvariant).Trim('-');
        return string.IsNullOrEmpty(s) ? "sablon" : s;
    }

    private async Task<string> GenerateUniqueCodeAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var baseCode = Slugify(name);
        var all = await _templateRepository.GetAllAsync(cancellationToken);
        var taken = all
            .Where(t => !excludeId.HasValue || t.Id != excludeId.Value)
            .Select(t => t.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidate = baseCode;
        var n = 2;
        while (taken.Contains(candidate))
        {
            candidate = $"{baseCode}-{n++}";
        }

        return candidate;
    }

    private async Task SaveTemplatePagesAsync(int templateId, IReadOnlyList<PageTemplatePageLineViewModel> lines, CancellationToken cancellationToken)
    {
        foreach (var line in lines.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Name))
        {
            await _templatePageRepository.CreateAsync(new PageTemplatePage
            {
                PageTemplateId = templateId,
                Name = line.Name.Trim(),
                DisplayOrder = line.DisplayOrder,
                HtmlContent = line.HtmlContent!.Trim(),
                PreviewImagePath = string.IsNullOrWhiteSpace(line.PreviewImagePath) ? null : line.PreviewImagePath.Trim()
            }, cancellationToken);
        }
    }

    private async Task SyncTemplatePagesAsync(int templateId, IReadOnlyList<PageTemplatePageLineViewModel> lines, CancellationToken cancellationToken)
    {
        var existing = await _templatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId",
            new { TemplateId = templateId },
            cancellationToken);

        var formIds = lines.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();
        foreach (var row in existing)
        {
            if (!formIds.Contains(row.Id))
            {
                await _templatePageRepository.DeleteAsync(row.Id, cancellationToken);
            }
        }

        foreach (var line in lines.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Name))
        {
            if (line.Id > 0)
            {
                var entity = await _templatePageRepository.GetByIdAsync(line.Id, cancellationToken);
                if (entity is null || entity.PageTemplateId != templateId) continue;

                entity.Name = line.Name.Trim();
                entity.DisplayOrder = line.DisplayOrder;
                entity.HtmlContent = line.HtmlContent!.Trim();
                if (line.PreviewImagePath is not null)
                {
                    entity.PreviewImagePath = string.IsNullOrWhiteSpace(line.PreviewImagePath)
                        ? null
                        : line.PreviewImagePath.Trim();
                }

                await _templatePageRepository.UpdateAsync(entity, cancellationToken);
            }
            else
            {
                await _templatePageRepository.CreateAsync(new PageTemplatePage
                {
                    PageTemplateId = templateId,
                    Name = line.Name.Trim(),
                    DisplayOrder = line.DisplayOrder,
                    HtmlContent = line.HtmlContent!.Trim(),
                    PreviewImagePath = string.IsNullOrWhiteSpace(line.PreviewImagePath) ? null : line.PreviewImagePath.Trim()
                }, cancellationToken);
            }
        }
    }

    private async Task ReplaceTemplatePagesAsync(int templateId, IReadOnlyList<PageTemplatePageLineViewModel> lines, CancellationToken cancellationToken)
    {
        var existing = await _templatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId",
            new { TemplateId = templateId },
            cancellationToken);

        foreach (var row in existing)
        {
            await _templatePageRepository.DeleteAsync(row.Id, cancellationToken);
        }

        await SaveTemplatePagesAsync(templateId, lines, cancellationToken);
    }

    private async Task ApplyPreviewImageAsync(int templateId, CancellationToken cancellationToken)
    {
        var file = Request.Form.Files.GetFile("previewImage");
        if (file is null || file.Length == 0) return;
        if (file.Length > MaxPreviewBytes) return;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp")) return;

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var relDir = Path.Combine("pictures", "template-previews");
        var physicalDir = Path.Combine(webRoot, relDir);
        Directory.CreateDirectory(physicalDir);

        var stored = $"{templateId}-{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalDir, stored);
        await using (var fs = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(fs, cancellationToken);
        }

        var webPath = "/" + relDir.Replace(Path.DirectorySeparatorChar, '/') + "/" + stored;

        var pages = await _templatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
            new { TemplateId = templateId },
            cancellationToken);

        var first = pages.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).FirstOrDefault();
        if (first is null) return;

        first.PreviewImagePath = webPath;
        await _templatePageRepository.UpdateAsync(first, cancellationToken);
    }

    private bool ValidateTemplatePages(PageTemplateEditViewModel model)
    {
        if (model.Pages.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "En az bir şablon içi sayfa gerekli.");
            return false;
        }

        for (var i = 0; i < model.Pages.Count; i++)
        {
            var line = model.Pages[i];
            if (string.IsNullOrWhiteSpace(line.Name))
            {
                ModelState.AddModelError(string.Empty, "Sayfa adı gerekli.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(line.HtmlContent))
            {
                ModelState.AddModelError(string.Empty, $"“{line.Name}” için HTML içeriği gerekli.");
                return false;
            }
        }

        return true;
    }
}

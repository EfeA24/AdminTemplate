using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageTemplatesController : Controller
{
    private const long MaxZipBytes = 5 * 1024 * 1024;
    private const long MaxHtmlFileBytes = 2 * 1024 * 1024;

    private readonly IPageTemplate _templateRepository;
    private readonly IPageTemplatePage _templatePageRepository;

    public PageTemplatesController(IPageTemplate templateRepository, IPageTemplatePage templatePageRepository)
    {
        _templateRepository = templateRepository;
        _templatePageRepository = templatePageRepository;
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
        return View("Form", new PageTemplateEditViewModel
        {
            Pages =
            [
                new PageTemplatePageLineViewModel { Name = "Sayfa 1", DisplayOrder = 0, HtmlContent = "" }
            ]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxZipBytes + MaxHtmlFileBytes * 20)]
    [RequestSizeLimit(MaxZipBytes + MaxHtmlFileBytes * 20)]
    public async Task<IActionResult> Create(PageTemplateEditViewModel model, IFormFile? zipArchive, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        NormalizeCode(model);
        await TryApplyZipAsync(model, zipArchive, cancellationToken);
        ApplyPerRowHtmlUploads(model);

        if (!ValidateTemplatePages(model))
            return View("Form", model);

        if (!ModelState.IsValid) return View("Form", model);

        if (await CodeExistsAsync(model.Code, excludeId: null, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kod başka bir şablonda kullanılıyor.");
            return View("Form", model);
        }

        var template = new PageTemplate
        {
            Name = model.Name.Trim(),
            Code = model.Code.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        var templateId = await _templateRepository.CreateAsync(template, cancellationToken);
        await SaveTemplatePagesAsync(templateId, model.Pages, cancellationToken);

        return RedirectToAction(nameof(Index));
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
                HtmlContent = line.HtmlContent!.Trim()
            }, cancellationToken);
        }
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

        ViewBag.DisplayName = "Şablonlar";
        return View("Form", new PageTemplateEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Pages = pages.Select(p => new PageTemplatePageLineViewModel
            {
                Id = p.Id,
                Name = p.Name,
                DisplayOrder = p.DisplayOrder,
                HtmlContent = p.HtmlContent
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxZipBytes + MaxHtmlFileBytes * 20)]
    [RequestSizeLimit(MaxZipBytes + MaxHtmlFileBytes * 20)]
    public async Task<IActionResult> Edit(int id, PageTemplateEditViewModel model, IFormFile? zipArchive, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Şablonlar";
        if (id != model.Id) return BadRequest();

        NormalizeCode(model);
        await TryApplyZipAsync(model, zipArchive, cancellationToken);
        ApplyPerRowHtmlUploads(model);

        if (!ValidateTemplatePages(model))
            return View("Form", model);

        if (!ModelState.IsValid) return View("Form", model);

        if (await CodeExistsAsync(model.Code, excludeId: id, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kod başka bir şablonda kullanılıyor.");
            return View("Form", model);
        }

        var entity = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = model.Name.Trim();
        entity.Code = model.Code.Trim();
        entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        entity.IsActive = model.IsActive;

        await _templateRepository.UpdateAsync(entity, cancellationToken);
        await SyncTemplatePagesAsync(id, model.Pages, cancellationToken);

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
        await _templateRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private static void NormalizeCode(PageTemplateEditViewModel model)
    {
        model.Code = model.Code.Trim().Replace(" ", "-", StringComparison.Ordinal);
    }

    private async Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        var all = await _templateRepository.GetAllAsync(cancellationToken);
        return all.Any(t =>
            string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase) &&
            (!excludeId.HasValue || t.Id != excludeId.Value));
    }

    private async Task TryApplyZipAsync(PageTemplateEditViewModel model, IFormFile? zipArchive, CancellationToken cancellationToken)
    {
        if (zipArchive is null || zipArchive.Length == 0) return;

        if (zipArchive.Length > MaxZipBytes)
        {
            ModelState.AddModelError(string.Empty, "ZIP dosyası en fazla 5 MB olabilir.");
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

        var extracted = ExtractHtmlEntriesFromZip(ms);
        if (extracted.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "ZIP içinde .html dosyası bulunamadı.");
            return;
        }

        model.Pages.Clear();
        var order = 0;
        foreach (var (name, html) in extracted)
        {
            model.Pages.Add(new PageTemplatePageLineViewModel
            {
                Id = 0,
                Name = name,
                DisplayOrder = order++,
                HtmlContent = html
            });
        }
    }

    private static IReadOnlyList<(string Name, string Html)> ExtractHtmlEntriesFromZip(Stream stream)
    {
        var list = new List<(string Name, string Html)>();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries.OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            if (!entry.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;

            var relative = entry.FullName.Replace('\\', '/');
            var safeName = Path.GetFileName(relative);
            if (string.IsNullOrEmpty(safeName) || safeName.Contains("..", StringComparison.Ordinal)) continue;

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var html = reader.ReadToEnd();
            var baseName = Path.GetFileNameWithoutExtension(safeName);
            list.Add((string.IsNullOrWhiteSpace(baseName) ? safeName : baseName, html));
        }

        return list;
    }

    private void ApplyPerRowHtmlUploads(PageTemplateEditViewModel model)
    {
        for (var i = 0; i < model.Pages.Count; i++)
        {
            var file = Request.Form.Files.GetFile($"HtmlFile_{i}");
            if (file is null || file.Length == 0) continue;
            if (file.Length > MaxHtmlFileBytes)
            {
                ModelState.AddModelError(string.Empty, $"Satır {i + 1}: HTML dosyası en fazla 2 MB olabilir.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".html", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ext, ".htm", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, $"Satır {i + 1}: yalnızca .html yükleyin.");
                continue;
            }

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            model.Pages[i].HtmlContent = reader.ReadToEnd();
        }
    }

    private bool ValidateTemplatePages(PageTemplateEditViewModel model)
    {
        if (model.Pages.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "En az bir şablon içi sayfa ekleyin.");
            return false;
        }

        for (var i = 0; i < model.Pages.Count; i++)
        {
            var line = model.Pages[i];
            if (string.IsNullOrWhiteSpace(line.Name))
            {
                ModelState.AddModelError($"Pages[{i}].Name", "Sayfa adı gerekli.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(line.HtmlContent))
            {
                ModelState.AddModelError($"Pages[{i}].HtmlContent", $"“{line.Name}” için HTML içeriği gerekli.");
                return false;
            }
        }

        return true;
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
                await _templatePageRepository.UpdateAsync(entity, cancellationToken);
            }
            else
            {
                await _templatePageRepository.CreateAsync(new PageTemplatePage
                {
                    PageTemplateId = templateId,
                    Name = line.Name.Trim(),
                    DisplayOrder = line.DisplayOrder,
                    HtmlContent = line.HtmlContent!.Trim()
                }, cancellationToken);
            }
        }
    }
}

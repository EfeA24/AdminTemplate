using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PagesController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageTemplate _pageTemplateRepository;
    private readonly IPageTemplatePage _pageTemplatePageRepository;
    private readonly IColorPalette _colorPaletteRepository;

    public PagesController(
        IPage pageRepository,
        IPageTemplate pageTemplateRepository,
        IPageTemplatePage pageTemplatePageRepository,
        IColorPalette colorPaletteRepository)
    {
        _pageRepository = pageRepository;
        _pageTemplateRepository = pageTemplateRepository;
        _pageTemplatePageRepository = pageTemplatePageRepository;
        _colorPaletteRepository = colorPaletteRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _pageRepository.GetAllAsync(cancellationToken);
        return View("~/Views/Pages/Index.cshtml", list);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        return page is null ? NotFound() : View("~/Views/Pages/Details.cshtml", page);
    }

    [HttpGet]
    public async Task<IActionResult> TemplatePages(int templateId, CancellationToken cancellationToken)
    {
        if (templateId <= 0)
        {
            return Json(Array.Empty<object>());
        }

        var pages = await _pageTemplatePageRepository.GetByConditionAsync(
            "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
            new { TemplateId = templateId },
            cancellationToken);

        return Json(pages.Select(p => new { id = p.Id, name = p.Name }));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulatePageFormLookupsAsync(cancellationToken);
        ViewBag.FormAction = "Create";
        return View("~/Views/Pages/Form.cshtml", new PageEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.FormAction = "Create";
        await PopulatePageFormLookupsAsync(cancellationToken);
        await ValidatePageDesignAsync(model, cancellationToken);
        if (!ModelState.IsValid) return View("~/Views/Pages/Form.cshtml", model);

        var entity = new Page
        {
            Name = model.Name,
            Slug = model.Slug,
            Title = model.Title,
            Description = model.Description,
            Width = model.Width,
            Height = model.Height,
            IsHomePage = model.IsHomePage,
            IsPublished = model.IsPublished,
            IsDeleted = model.IsDeleted,
            DisplayOrder = model.DisplayOrder,
            PageTemplatePageId = model.PageTemplatePageId,
            ColorPaletteId = model.ColorPaletteId,
            CreatedDate = DateTime.UtcNow
        };

        await _pageRepository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        var vm = new PageEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            Title = entity.Title,
            Description = entity.Description,
            Width = entity.Width,
            Height = entity.Height,
            IsHomePage = entity.IsHomePage,
            IsPublished = entity.IsPublished,
            IsDeleted = entity.IsDeleted,
            DisplayOrder = entity.DisplayOrder,
            PageTemplatePageId = entity.PageTemplatePageId,
            ColorPaletteId = entity.ColorPaletteId
        };

        if (entity.PageTemplatePageId is int tppId)
        {
            var tpp = await _pageTemplatePageRepository.GetByIdAsync(tppId, cancellationToken);
            if (tpp is not null)
            {
                vm.PageTemplateId = tpp.PageTemplateId;
            }
        }

        await PopulatePageFormLookupsAsync(cancellationToken, vm.PageTemplateId);
        ViewBag.FormAction = "Edit";
        return View("~/Views/Pages/Form.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();

        await PopulatePageFormLookupsAsync(cancellationToken, model.PageTemplateId);
        await ValidatePageDesignAsync(model, cancellationToken);
        if (!ModelState.IsValid) return View("~/Views/Pages/Form.cshtml", model);

        var entity = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = model.Name;
        entity.Slug = model.Slug;
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.Width = model.Width;
        entity.Height = model.Height;
        entity.IsHomePage = model.IsHomePage;
        entity.IsPublished = model.IsPublished;
        entity.IsDeleted = model.IsDeleted;
        entity.DisplayOrder = model.DisplayOrder;
        entity.PageTemplatePageId = model.PageTemplatePageId;
        entity.ColorPaletteId = model.ColorPaletteId;
        entity.UpdatedDate = DateTime.UtcNow;

        await _pageRepository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _pageRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Pages/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _pageRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulatePageFormLookupsAsync(CancellationToken cancellationToken, int? selectedTemplateId = null)
    {
        var templates = (await _pageTemplateRepository.GetAllAsync(cancellationToken))
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToList();
        ViewBag.PageTemplates = templates;

        var palettes = (await _colorPaletteRepository.GetAllAsync(cancellationToken))
            .OrderBy(p => p.Id)
            .ToList();
        ViewBag.ColorPalettes = palettes;

        if (selectedTemplateId is > 0)
        {
            ViewBag.TemplateInnerPages = await _pageTemplatePageRepository.GetByConditionAsync(
                "PageTemplateId = @TemplateId ORDER BY DisplayOrder, Id",
                new { TemplateId = selectedTemplateId.Value },
                cancellationToken);
        }
        else
        {
            ViewBag.TemplateInnerPages = Array.Empty<PageTemplatePage>();
        }
    }

    private async Task ValidatePageDesignAsync(PageEditViewModel model, CancellationToken cancellationToken)
    {
        if (!model.ColorPaletteId.HasValue || model.ColorPaletteId <= 0)
        {
            ModelState.AddModelError(nameof(model.ColorPaletteId), "Renk paleti seçin.");
        }

        var templatePageCount = (await _pageTemplatePageRepository.GetAllAsync(cancellationToken)).Count;
        if (templatePageCount > 0)
        {
            if (!model.PageTemplatePageId.HasValue || model.PageTemplatePageId <= 0)
            {
                ModelState.AddModelError(nameof(model.PageTemplatePageId), "Şablon içi sayfa seçin.");
            }
            else
            {
                var tpp = await _pageTemplatePageRepository.GetByIdAsync(model.PageTemplatePageId.Value, cancellationToken);
                if (tpp is null)
                {
                    ModelState.AddModelError(nameof(model.PageTemplatePageId), "Geçersiz şablon içi sayfa.");
                }
            }
        }
    }
}

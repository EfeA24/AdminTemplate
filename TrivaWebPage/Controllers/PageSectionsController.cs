using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageSectionsController : Controller
{
    private readonly IPageSection _sectionRepository;
    private readonly IPage _pageRepository;

    public PageSectionsController(IPageSection sectionRepository, IPage pageRepository)
    {
        _sectionRepository = sectionRepository;
        _pageRepository = pageRepository;
    }

    public async Task<IActionResult> Index(int? pageId, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Sections";
        ViewBag.PageId = pageId;

        if (pageId.HasValue)
        {
            var filtered = await _sectionRepository.GetByConditionAsync("PageId = @PageId", new { PageId = pageId.Value }, cancellationToken);
            return View("~/Views/Shared/AdminCrud/Index.cshtml", filtered);
        }

        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _sectionRepository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Sections";
        var entity = await _sectionRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? pageId, CancellationToken cancellationToken)
    {
        await PopulatePagesAsync(cancellationToken, pageId);
        ViewBag.DisplayName = "Page Sections";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageSectionEditViewModel { PageId = pageId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageSectionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Sections";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulatePagesAsync(cancellationToken, model.PageId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new PageSection
        {
            PageId = model.PageId,
            Name = model.Name,
            SectionType = model.SectionType,
            DisplayOrder = model.DisplayOrder,
            CssClass = model.CssClass,
            InlineStyle = model.InlineStyle,
            IsVisible = model.IsVisible
        };

        await _sectionRepository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _sectionRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        await PopulatePagesAsync(cancellationToken, entity.PageId);
        ViewBag.DisplayName = "Page Sections";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageSectionEditViewModel
        {
            Id = entity.Id,
            PageId = entity.PageId,
            Name = entity.Name,
            SectionType = entity.SectionType,
            DisplayOrder = entity.DisplayOrder,
            CssClass = entity.CssClass,
            InlineStyle = entity.InlineStyle,
            IsVisible = entity.IsVisible
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageSectionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Sections";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulatePagesAsync(cancellationToken, model.PageId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _sectionRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageId = model.PageId;
        entity.Name = model.Name;
        entity.SectionType = model.SectionType;
        entity.DisplayOrder = model.DisplayOrder;
        entity.CssClass = model.CssClass;
        entity.InlineStyle = model.InlineStyle;
        entity.IsVisible = model.IsVisible;

        await _sectionRepository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Sections";
        var entity = await _sectionRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _sectionRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        await _sectionRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { pageId = entity.PageId });
    }

    private async Task PopulatePagesAsync(CancellationToken cancellationToken, int? selectedPageId)
    {
        var pages = await _pageRepository.GetAllAsync(cancellationToken);
        ViewBag.PageId = new SelectList(pages, "Id", "Name", selectedPageId);
    }
}

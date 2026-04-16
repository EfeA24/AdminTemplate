using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PageComponentsController : Controller
{
    private readonly IPageComponent _componentRepository;
    private readonly IPageSection _sectionRepository;

    public PageComponentsController(IPageComponent componentRepository, IPageSection sectionRepository)
    {
        _componentRepository = componentRepository;
        _sectionRepository = sectionRepository;
    }

    public async Task<IActionResult> Index(int? sectionId, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Components";
        ViewBag.SectionId = sectionId;

        if (sectionId.HasValue)
        {
            var filtered = await _componentRepository.GetByConditionAsync("PageSectionId = @SectionId", new { SectionId = sectionId.Value }, cancellationToken);
            return View("~/Views/Shared/AdminCrud/Index.cshtml", filtered);
        }

        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _componentRepository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Components";
        var entity = await _componentRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? sectionId, CancellationToken cancellationToken)
    {
        await PopulateSectionsAsync(cancellationToken, sectionId);
        ViewBag.DisplayName = "Page Components";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageComponentEditViewModel { PageSectionId = sectionId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Components";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSectionsAsync(cancellationToken, model.PageSectionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new PageComponent
        {
            PageSectionId = model.PageSectionId,
            Name = model.Name,
            ComponentType = model.ComponentType,
            DisplayOrder = model.DisplayOrder,
            X = model.X,
            Y = model.Y,
            Width = model.Width,
            Height = model.Height,
            CssClass = model.CssClass,
            InlineStyle = model.InlineStyle,
            IsVisible = model.IsVisible,
            CreatedDate = DateTime.UtcNow
        };

        await _componentRepository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index), new { sectionId = model.PageSectionId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _componentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        await PopulateSectionsAsync(cancellationToken, entity.PageSectionId);
        ViewBag.DisplayName = "Page Components";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageComponentEditViewModel
        {
            Id = entity.Id,
            PageSectionId = entity.PageSectionId,
            Name = entity.Name,
            ComponentType = entity.ComponentType,
            DisplayOrder = entity.DisplayOrder,
            X = entity.X,
            Y = entity.Y,
            Width = entity.Width,
            Height = entity.Height,
            CssClass = entity.CssClass,
            InlineStyle = entity.InlineStyle,
            IsVisible = entity.IsVisible
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Components";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSectionsAsync(cancellationToken, model.PageSectionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _componentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageSectionId = model.PageSectionId;
        entity.Name = model.Name;
        entity.ComponentType = model.ComponentType;
        entity.DisplayOrder = model.DisplayOrder;
        entity.X = model.X;
        entity.Y = model.Y;
        entity.Width = model.Width;
        entity.Height = model.Height;
        entity.CssClass = model.CssClass;
        entity.InlineStyle = model.InlineStyle;
        entity.IsVisible = model.IsVisible;
        entity.UpdatedDate = DateTime.UtcNow;

        await _componentRepository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index), new { sectionId = model.PageSectionId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Page Components";
        var entity = await _componentRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _componentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        await _componentRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { sectionId = entity.PageSectionId });
    }

    private async Task PopulateSectionsAsync(CancellationToken cancellationToken, int? selectedSectionId)
    {
        var sections = await _sectionRepository.GetAllAsync(cancellationToken);
        ViewBag.PageSectionId = new SelectList(sections, "Id", "Name", selectedSectionId);
    }
}

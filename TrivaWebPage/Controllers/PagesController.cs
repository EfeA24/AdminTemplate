using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class PagesController : Controller
{
    private readonly IPage _pageRepository;

    public PagesController(IPage pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Pages";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _pageRepository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Pages";
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        return page is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", page);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Pages";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Pages";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

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

        ViewBag.DisplayName = "Pages";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new PageEditViewModel
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
            DisplayOrder = entity.DisplayOrder
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Pages";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

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
        entity.UpdatedDate = DateTime.UtcNow;

        await _pageRepository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Pages";
        var entity = await _pageRepository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _pageRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

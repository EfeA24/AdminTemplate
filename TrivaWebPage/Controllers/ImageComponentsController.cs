using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.Contents;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class ImageComponentsController : Controller
{
    private readonly IImageComponent _repository;
    private readonly IPageComponent _pageComponentRepository;
    private readonly IMediaFile _mediaFileRepository;

    public ImageComponentsController(IImageComponent repository, IPageComponent pageComponentRepository, IMediaFile mediaFileRepository)
    {
        _repository = repository;
        _pageComponentRepository = pageComponentRepository;
        _mediaFileRepository = mediaFileRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Image Components";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Image Components";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateSelectListsAsync(cancellationToken, null, null);
        ViewBag.DisplayName = "Image Components";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ImageComponentEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ImageComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Image Components";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.MediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new ImageComponent
        {
            PageComponentId = model.PageComponentId,
            MediaFileId = model.MediaFileId,
            AltText = model.AltText,
            FitType = model.FitType,
            BorderRadius = model.BorderRadius,
            HasShadow = model.HasShadow
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateSelectListsAsync(cancellationToken, entity.PageComponentId, entity.MediaFileId);

        ViewBag.DisplayName = "Image Components";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ImageComponentEditViewModel
        {
            Id = entity.Id,
            PageComponentId = entity.PageComponentId,
            MediaFileId = entity.MediaFileId,
            AltText = entity.AltText,
            FitType = entity.FitType,
            BorderRadius = entity.BorderRadius,
            HasShadow = entity.HasShadow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ImageComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Image Components";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.MediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageComponentId = model.PageComponentId;
        entity.MediaFileId = model.MediaFileId;
        entity.AltText = model.AltText;
        entity.FitType = model.FitType;
        entity.BorderRadius = model.BorderRadius;
        entity.HasShadow = model.HasShadow;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Image Components";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Delete.cshtml", entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectListsAsync(CancellationToken cancellationToken, int? selectedPageComponentId, int? selectedMediaFileId)
    {
        var components = await _pageComponentRepository.GetAllAsync(cancellationToken);
        var mediaFiles = await _mediaFileRepository.GetAllAsync(cancellationToken);

        ViewBag.PageComponentId = new SelectList(components, "Id", "Name", selectedPageComponentId);
        ViewBag.MediaFileId = new SelectList(mediaFiles, "Id", "FileName", selectedMediaFileId);
    }
}

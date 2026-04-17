using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class MediaFilesController : Controller
{
    private readonly IMediaFile _repository;

    public MediaFilesController(IMediaFile repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(ImagesController.Index), "Images");
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Media Files";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Media Files";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new MediaFileEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MediaFileEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Media Files";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

        var entity = new MediaFile
        {
            FileName = model.FileName,
            OriginalFileName = model.OriginalFileName,
            FilePath = model.FilePath,
            AltText = model.AltText,
            ContentType = model.ContentType,
            FileSize = model.FileSize,
            FileExtension = model.FileExtension,
            Width = model.Width,
            Height = model.Height,
            UploadedDate = DateTime.UtcNow
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        ViewBag.DisplayName = "Media Files";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new MediaFileEditViewModel
        {
            Id = entity.Id,
            FileName = entity.FileName,
            OriginalFileName = entity.OriginalFileName,
            FilePath = entity.FilePath,
            AltText = entity.AltText,
            ContentType = entity.ContentType,
            FileSize = entity.FileSize,
            FileExtension = entity.FileExtension,
            Width = entity.Width,
            Height = entity.Height
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MediaFileEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Media Files";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.FileName = model.FileName;
        entity.OriginalFileName = model.OriginalFileName;
        entity.FilePath = model.FilePath;
        entity.AltText = model.AltText;
        entity.ContentType = model.ContentType;
        entity.FileSize = model.FileSize;
        entity.FileExtension = model.FileExtension;
        entity.Width = model.Width;
        entity.Height = model.Height;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Media Files";
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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardDefinitionsController : Controller
{
    private readonly ICardDefinition _repository;
    private readonly IMediaFile _mediaFileRepository;

    public CardDefinitionsController(ICardDefinition repository, IMediaFile mediaFileRepository)
    {
        _repository = repository;
        _mediaFileRepository = mediaFileRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateMediaFilesAsync(cancellationToken, null);
        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardDefinitionEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateMediaFilesAsync(cancellationToken, model.PreviewMediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new CardDefinition
        {
            Name = model.Name,
            Code = model.Code,
            CardType = model.CardType,
            Description = model.Description,
            PreviewMediaFileId = model.PreviewMediaFileId,
            IsActive = model.IsActive
        };
        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateMediaFilesAsync(cancellationToken, entity.PreviewMediaFileId);

        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardDefinitionEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            CardType = entity.CardType,
            Description = entity.Description,
            PreviewMediaFileId = entity.PreviewMediaFileId,
            IsActive = entity.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateMediaFilesAsync(cancellationToken, model.PreviewMediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = model.Name;
        entity.Code = model.Code;
        entity.CardType = model.CardType;
        entity.Description = model.Description;
        entity.PreviewMediaFileId = model.PreviewMediaFileId;
        entity.IsActive = model.IsActive;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
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

    private async Task PopulateMediaFilesAsync(CancellationToken cancellationToken, int? selectedPreviewMediaFileId)
    {
        var mediaFiles = await _mediaFileRepository.GetAllAsync(cancellationToken);
        ViewBag.PreviewMediaFileId = new SelectList(mediaFiles, "Id", "FileName", selectedPreviewMediaFileId);
    }
}

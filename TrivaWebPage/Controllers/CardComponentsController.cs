using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.Contents;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardComponentsController : Controller
{
    private readonly ICardComponent _repository;
    private readonly IPageComponent _pageComponentRepository;
    private readonly ICardDefinition _cardDefinitionRepository;
    private readonly IMediaFile _mediaFileRepository;

    public CardComponentsController(
        ICardComponent repository,
        IPageComponent pageComponentRepository,
        ICardDefinition cardDefinitionRepository,
        IMediaFile mediaFileRepository)
    {
        _repository = repository;
        _pageComponentRepository = pageComponentRepository;
        _cardDefinitionRepository = cardDefinitionRepository;
        _mediaFileRepository = mediaFileRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Components";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Components";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateSelectListsAsync(cancellationToken, null, null, null);
        ViewBag.DisplayName = "Card Components";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardComponentEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Components";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.CardDefinitionId, model.MediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new CardComponent
        {
            PageComponentId = model.PageComponentId,
            CardDefinitionId = model.CardDefinitionId,
            Title = model.Title,
            Subtitle = model.Subtitle,
            Description = model.Description,
            MediaFileId = model.MediaFileId,
            BackgroundColor = model.BackgroundColor,
            TextColor = model.TextColor,
            BorderColor = model.BorderColor,
            ShowImage = model.ShowImage,
            ShowButton = model.ShowButton
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateSelectListsAsync(cancellationToken, entity.PageComponentId, entity.CardDefinitionId, entity.MediaFileId);

        ViewBag.DisplayName = "Card Components";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardComponentEditViewModel
        {
            Id = entity.Id,
            PageComponentId = entity.PageComponentId,
            CardDefinitionId = entity.CardDefinitionId,
            Title = entity.Title,
            Subtitle = entity.Subtitle,
            Description = entity.Description,
            MediaFileId = entity.MediaFileId,
            BackgroundColor = entity.BackgroundColor,
            TextColor = entity.TextColor,
            BorderColor = entity.BorderColor,
            ShowImage = entity.ShowImage,
            ShowButton = entity.ShowButton
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Components";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.CardDefinitionId, model.MediaFileId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageComponentId = model.PageComponentId;
        entity.CardDefinitionId = model.CardDefinitionId;
        entity.Title = model.Title;
        entity.Subtitle = model.Subtitle;
        entity.Description = model.Description;
        entity.MediaFileId = model.MediaFileId;
        entity.BackgroundColor = model.BackgroundColor;
        entity.TextColor = model.TextColor;
        entity.BorderColor = model.BorderColor;
        entity.ShowImage = model.ShowImage;
        entity.ShowButton = model.ShowButton;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Components";
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

    private async Task PopulateSelectListsAsync(
        CancellationToken cancellationToken,
        int? selectedPageComponentId,
        int? selectedCardDefinitionId,
        int? selectedMediaFileId)
    {
        var components = await _pageComponentRepository.GetAllAsync(cancellationToken);
        var definitions = await _cardDefinitionRepository.GetAllAsync(cancellationToken);
        var mediaFiles = await _mediaFileRepository.GetAllAsync(cancellationToken);

        ViewBag.PageComponentId = new SelectList(components, "Id", "Name", selectedPageComponentId);
        ViewBag.CardDefinitionId = new SelectList(definitions, "Id", "Name", selectedCardDefinitionId);
        ViewBag.MediaFileId = new SelectList(mediaFiles, "Id", "FileName", selectedMediaFileId);
    }
}

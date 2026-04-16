using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardButtonsController : Controller
{
    private readonly ICardButton _repository;
    private readonly ICardComponent _cardComponentRepository;
    private readonly IActionDefinition _actionDefinitionRepository;

    public CardButtonsController(ICardButton repository, ICardComponent cardComponentRepository, IActionDefinition actionDefinitionRepository)
    {
        _repository = repository;
        _cardComponentRepository = cardComponentRepository;
        _actionDefinitionRepository = actionDefinitionRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Buttons";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Buttons";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateSelectListsAsync(cancellationToken, null, null);
        ViewBag.DisplayName = "Card Buttons";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardButtonEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardButtonEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Buttons";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.CardComponentId, model.ActionDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new CardButton
        {
            CardComponentId = model.CardComponentId,
            Text = model.Text,
            Icon = model.Icon,
            BackgroundColor = model.BackgroundColor,
            TextColor = model.TextColor,
            BorderColor = model.BorderColor,
            DisplayOrder = model.DisplayOrder,
            ActionDefinitionId = model.ActionDefinitionId
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateSelectListsAsync(cancellationToken, entity.CardComponentId, entity.ActionDefinitionId);

        ViewBag.DisplayName = "Card Buttons";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardButtonEditViewModel
        {
            Id = entity.Id,
            CardComponentId = entity.CardComponentId,
            Text = entity.Text,
            Icon = entity.Icon,
            BackgroundColor = entity.BackgroundColor,
            TextColor = entity.TextColor,
            BorderColor = entity.BorderColor,
            DisplayOrder = entity.DisplayOrder,
            ActionDefinitionId = entity.ActionDefinitionId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardButtonEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Buttons";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.CardComponentId, model.ActionDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.CardComponentId = model.CardComponentId;
        entity.Text = model.Text;
        entity.Icon = model.Icon;
        entity.BackgroundColor = model.BackgroundColor;
        entity.TextColor = model.TextColor;
        entity.BorderColor = model.BorderColor;
        entity.DisplayOrder = model.DisplayOrder;
        entity.ActionDefinitionId = model.ActionDefinitionId;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Buttons";
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

    private async Task PopulateSelectListsAsync(CancellationToken cancellationToken, int? selectedCardComponentId, int? selectedActionDefinitionId)
    {
        var cardComponents = await _cardComponentRepository.GetAllAsync(cancellationToken);
        var actions = await _actionDefinitionRepository.GetAllAsync(cancellationToken);
        ViewBag.CardComponentId = new SelectList(cardComponents, "Id", "Title", selectedCardComponentId);
        ViewBag.ActionDefinitionId = new SelectList(actions, "Id", "Name", selectedActionDefinitionId);
    }
}

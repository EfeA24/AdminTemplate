using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardFieldValuesController : Controller
{
    private readonly ICardFieldValue _repository;
    private readonly ICardComponent _cardComponentRepository;
    private readonly ICardFieldDefinition _cardFieldDefinitionRepository;

    public CardFieldValuesController(ICardFieldValue repository, ICardComponent cardComponentRepository, ICardFieldDefinition cardFieldDefinitionRepository)
    {
        _repository = repository;
        _cardComponentRepository = cardComponentRepository;
        _cardFieldDefinitionRepository = cardFieldDefinitionRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Values";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Values";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateSelectListsAsync(cancellationToken, null, null);
        ViewBag.DisplayName = "Card Field Values";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardFieldValueEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardFieldValueEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Values";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.CardComponentId, model.CardFieldDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new CardFieldValue
        {
            CardComponentId = model.CardComponentId,
            CardFieldDefinitionId = model.CardFieldDefinitionId,
            FieldValue = model.FieldValue
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateSelectListsAsync(cancellationToken, entity.CardComponentId, entity.CardFieldDefinitionId);

        ViewBag.DisplayName = "Card Field Values";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardFieldValueEditViewModel
        {
            Id = entity.Id,
            CardComponentId = entity.CardComponentId,
            CardFieldDefinitionId = entity.CardFieldDefinitionId,
            FieldValue = entity.FieldValue
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardFieldValueEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Values";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.CardComponentId, model.CardFieldDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.CardComponentId = model.CardComponentId;
        entity.CardFieldDefinitionId = model.CardFieldDefinitionId;
        entity.FieldValue = model.FieldValue;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Values";
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

    private async Task PopulateSelectListsAsync(CancellationToken cancellationToken, int? selectedCardComponentId, int? selectedCardFieldDefinitionId)
    {
        var cardComponents = await _cardComponentRepository.GetAllAsync(cancellationToken);
        var fieldDefinitions = await _cardFieldDefinitionRepository.GetAllAsync(cancellationToken);
        ViewBag.CardComponentId = new SelectList(cardComponents, "Id", "Title", selectedCardComponentId);
        ViewBag.CardFieldDefinitionId = new SelectList(fieldDefinitions, "Id", "FieldName", selectedCardFieldDefinitionId);
    }
}

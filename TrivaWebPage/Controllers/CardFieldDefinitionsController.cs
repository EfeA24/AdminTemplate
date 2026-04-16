using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardFieldDefinitionsController : Controller
{
    private readonly ICardFieldDefinition _repository;
    private readonly ICardDefinition _cardDefinitionRepository;

    public CardFieldDefinitionsController(ICardFieldDefinition repository, ICardDefinition cardDefinitionRepository)
    {
        _repository = repository;
        _cardDefinitionRepository = cardDefinitionRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Definitions";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Definitions";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateCardDefinitionsAsync(cancellationToken, null);
        ViewBag.DisplayName = "Card Field Definitions";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardFieldDefinitionEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardFieldDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Definitions";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateCardDefinitionsAsync(cancellationToken, model.CardDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new CardFieldDefinition
        {
            CardDefinitionId = model.CardDefinitionId,
            FieldName = model.FieldName,
            FieldKey = model.FieldKey,
            FieldType = model.FieldType,
            IsRequired = model.IsRequired,
            DisplayOrder = model.DisplayOrder
        };
        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulateCardDefinitionsAsync(cancellationToken, entity.CardDefinitionId);

        ViewBag.DisplayName = "Card Field Definitions";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new CardFieldDefinitionEditViewModel
        {
            Id = entity.Id,
            CardDefinitionId = entity.CardDefinitionId,
            FieldName = entity.FieldName,
            FieldKey = entity.FieldKey,
            FieldType = entity.FieldType,
            IsRequired = entity.IsRequired,
            DisplayOrder = entity.DisplayOrder
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardFieldDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Definitions";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateCardDefinitionsAsync(cancellationToken, model.CardDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.CardDefinitionId = model.CardDefinitionId;
        entity.FieldName = model.FieldName;
        entity.FieldKey = model.FieldKey;
        entity.FieldType = model.FieldType;
        entity.IsRequired = model.IsRequired;
        entity.DisplayOrder = model.DisplayOrder;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Field Definitions";
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

    private async Task PopulateCardDefinitionsAsync(CancellationToken cancellationToken, int? selectedCardDefinitionId)
    {
        var definitions = await _cardDefinitionRepository.GetAllAsync(cancellationToken);
        ViewBag.CardDefinitionId = new SelectList(definitions, "Id", "Name", selectedCardDefinitionId);
    }
}

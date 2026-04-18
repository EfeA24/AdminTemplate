using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Helpers;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardDefinitionsController : Controller
{
    private readonly ICardDefinition _repository;

    public CardDefinitionsController(ICardDefinition repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        return View("~/Views/CardDefinitions/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/CardDefinitions/Details.cshtml", entity);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Create";
        return View("~/Views/CardDefinitions/Form.cshtml", new CardDefinitionEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            return View("~/Views/CardDefinitions/Form.cshtml", model);
        }

        var preview = NormalizePreviewHtml(model.PreviewHtml);
        var entity = new CardDefinition
        {
            Name = model.Name,
            Code = model.Code,
            CardType = model.CardType,
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            PreviewHtml = preview,
            PreviewMediaFileId = null,
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

        ViewBag.DisplayName = "Card Definitions";
        ViewBag.FormAction = "Edit";
        return View("~/Views/CardDefinitions/Form.cshtml", new CardDefinitionEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            CardType = entity.CardType,
            Description = entity.Description,
            PreviewHtml = entity.PreviewHtml,
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
            return View("~/Views/CardDefinitions/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = model.Name;
        entity.Code = model.Code;
        entity.CardType = model.CardType;
        entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        entity.PreviewHtml = NormalizePreviewHtml(model.PreviewHtml);
        entity.PreviewMediaFileId = null;
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

    private static string? NormalizePreviewHtml(string? raw)
    {
        var sanitized = AdminHtmlSanitizer.Sanitize(raw ?? string.Empty);
        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
    }
}

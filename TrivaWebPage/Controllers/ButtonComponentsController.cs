using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.Contents;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class ButtonComponentsController : Controller
{
    private readonly IButtonComponent _repository;
    private readonly IPageComponent _pageComponentRepository;
    private readonly IActionDefinition _actionDefinitionRepository;

    public ButtonComponentsController(IButtonComponent repository, IPageComponent pageComponentRepository, IActionDefinition actionDefinitionRepository)
    {
        _repository = repository;
        _pageComponentRepository = pageComponentRepository;
        _actionDefinitionRepository = actionDefinitionRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Button Components";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Button Components";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateSelectListsAsync(cancellationToken, null, null);
        ViewBag.DisplayName = "Button Components";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ButtonComponentEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ButtonComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Button Components";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.ActionDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new ButtonComponent
        {
            PageComponentId = model.PageComponentId,
            Text = model.Text,
            Icon = model.Icon,
            BackgroundColor = model.BackgroundColor,
            TextColor = model.TextColor,
            BorderColor = model.BorderColor,
            SizeType = model.SizeType,
            StyleType = model.StyleType,
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
        await PopulateSelectListsAsync(cancellationToken, entity.PageComponentId, entity.ActionDefinitionId);

        ViewBag.DisplayName = "Button Components";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ButtonComponentEditViewModel
        {
            Id = entity.Id,
            PageComponentId = entity.PageComponentId,
            Text = entity.Text,
            Icon = entity.Icon,
            BackgroundColor = entity.BackgroundColor,
            TextColor = entity.TextColor,
            BorderColor = entity.BorderColor,
            SizeType = entity.SizeType,
            StyleType = entity.StyleType,
            ActionDefinitionId = entity.ActionDefinitionId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ButtonComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Button Components";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(cancellationToken, model.PageComponentId, model.ActionDefinitionId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageComponentId = model.PageComponentId;
        entity.Text = model.Text;
        entity.Icon = model.Icon;
        entity.BackgroundColor = model.BackgroundColor;
        entity.TextColor = model.TextColor;
        entity.BorderColor = model.BorderColor;
        entity.SizeType = model.SizeType;
        entity.StyleType = model.StyleType;
        entity.ActionDefinitionId = model.ActionDefinitionId;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Button Components";
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

    private async Task PopulateSelectListsAsync(CancellationToken cancellationToken, int? selectedPageComponentId, int? selectedActionDefinitionId)
    {
        var components = await _pageComponentRepository.GetAllAsync(cancellationToken);
        var actions = await _actionDefinitionRepository.GetAllAsync(cancellationToken);
        ViewBag.PageComponentId = new SelectList(components, "Id", "Name", selectedPageComponentId);
        ViewBag.ActionDefinitionId = new SelectList(actions, "Id", "Name", selectedActionDefinitionId);
    }
}

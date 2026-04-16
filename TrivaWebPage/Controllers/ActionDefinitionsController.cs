using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class ActionDefinitionsController : Controller
{
    private readonly IActionDefinition _repository;

    public ActionDefinitionsController(IActionDefinition repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Action Definitions";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Action Definitions";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DisplayName = "Action Definitions";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ActionDefinitionEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActionDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Action Definitions";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

        var entity = new ActionDefinition
        {
            Name = model.Name,
            ActionType = model.ActionType,
            Url = model.Url,
            Target = model.Target,
            FunctionName = model.FunctionName,
            ParametersJson = model.ParametersJson,
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

        ViewBag.DisplayName = "Action Definitions";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new ActionDefinitionEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            ActionType = entity.ActionType,
            Url = entity.Url,
            Target = entity.Target,
            FunctionName = entity.FunctionName,
            ParametersJson = entity.ParametersJson,
            IsActive = entity.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActionDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Action Definitions";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("~/Views/Shared/AdminCrud/Form.cshtml", model);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        entity.Name = model.Name;
        entity.ActionType = model.ActionType;
        entity.Url = model.Url;
        entity.Target = model.Target;
        entity.FunctionName = model.FunctionName;
        entity.ParametersJson = model.ParametersJson;
        entity.IsActive = model.IsActive;
        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Action Definitions";
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

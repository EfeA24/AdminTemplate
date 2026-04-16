using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrivaWebPage.Abstractions.ContentAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.Contents;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class TextComponentsController : Controller
{
    private readonly ITextComponent _repository;
    private readonly IPageComponent _pageComponentRepository;

    public TextComponentsController(ITextComponent repository, IPageComponent pageComponentRepository)
    {
        _repository = repository;
        _pageComponentRepository = pageComponentRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Text Components";
        return View("~/Views/Shared/AdminCrud/Index.cshtml", await _repository.GetAllAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Text Components";
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : View("~/Views/Shared/AdminCrud/Details.cshtml", entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulatePageComponentsAsync(cancellationToken, null);
        ViewBag.DisplayName = "Text Components";
        ViewBag.FormAction = "Create";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new TextComponentEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TextComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Text Components";
        ViewBag.FormAction = "Create";
        if (!ModelState.IsValid)
        {
            await PopulatePageComponentsAsync(cancellationToken, model.PageComponentId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = new TextComponent
        {
            PageComponentId = model.PageComponentId,
            Content = model.Content,
            FontFamily = model.FontFamily,
            FontSize = model.FontSize,
            FontWeight = model.FontWeight,
            TextColor = model.TextColor,
            TextAlign = model.TextAlign,
            IsBold = model.IsBold,
            IsItalic = model.IsItalic,
            IsUnderline = model.IsUnderline
        };

        await _repository.CreateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        await PopulatePageComponentsAsync(cancellationToken, entity.PageComponentId);

        ViewBag.DisplayName = "Text Components";
        ViewBag.FormAction = "Edit";
        return View("~/Views/Shared/AdminCrud/Form.cshtml", new TextComponentEditViewModel
        {
            Id = entity.Id,
            PageComponentId = entity.PageComponentId,
            Content = entity.Content,
            FontFamily = entity.FontFamily,
            FontSize = entity.FontSize,
            FontWeight = entity.FontWeight,
            TextColor = entity.TextColor,
            TextAlign = entity.TextAlign,
            IsBold = entity.IsBold,
            IsItalic = entity.IsItalic,
            IsUnderline = entity.IsUnderline
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TextComponentEditViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Text Components";
        ViewBag.FormAction = "Edit";
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulatePageComponentsAsync(cancellationToken, model.PageComponentId);
            return View("~/Views/Shared/AdminCrud/Form.cshtml", model);
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return NotFound();

        entity.PageComponentId = model.PageComponentId;
        entity.Content = model.Content;
        entity.FontFamily = model.FontFamily;
        entity.FontSize = model.FontSize;
        entity.FontWeight = model.FontWeight;
        entity.TextColor = model.TextColor;
        entity.TextAlign = model.TextAlign;
        entity.IsBold = model.IsBold;
        entity.IsItalic = model.IsItalic;
        entity.IsUnderline = model.IsUnderline;

        await _repository.UpdateAsync(entity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        ViewBag.DisplayName = "Text Components";
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

    private async Task PopulatePageComponentsAsync(CancellationToken cancellationToken, int? selectedPageComponentId)
    {
        var components = await _pageComponentRepository.GetAllAsync(cancellationToken);
        ViewBag.PageComponentId = new SelectList(components, "Id", "Name", selectedPageComponentId);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateJson([FromBody] CardDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { ok = false, errors = FlattenErrors(ModelState) });
        }

        var utc = DateTime.UtcNow;
        var entity = new CardDefinition
        {
            Name = model.Name.Trim(),
            Code = model.Code.Trim(),
            PreviewImageUrl = string.IsNullOrWhiteSpace(model.PreviewImageUrl)
                ? "/pictures/placeholder-card.svg"
                : model.PreviewImageUrl.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            IsSystem = false,
            CreatedDate = utc,
            UpdatedDate = utc
        };

        try
        {
            await _repository.CreateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, errors = new[] { ex.Message } });
        }

        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Consumes("application/json")]
    public async Task<IActionResult> EditJson(int id, [FromBody] CardDefinitionEditViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return Json(new { ok = false, errors = new[] { "Geçersiz istek." } });
        }

        if (!ModelState.IsValid)
        {
            return Json(new { ok = false, errors = FlattenErrors(ModelState) });
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Json(new { ok = false, errors = new[] { "Kayıt bulunamadı." } });
        }

        if (entity.IsSystem)
        {
            entity.Name = model.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            entity.PreviewImageUrl = string.IsNullOrWhiteSpace(model.PreviewImageUrl)
                ? "/pictures/placeholder-card.svg"
                : model.PreviewImageUrl.Trim();
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            entity.Name = model.Name.Trim();
            entity.Code = model.Code.Trim();
            entity.PreviewImageUrl = string.IsNullOrWhiteSpace(model.PreviewImageUrl)
                ? "/pictures/placeholder-card.svg"
                : model.PreviewImageUrl.Trim();
            entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            entity.UpdatedDate = DateTime.UtcNow;
        }

        try
        {
            await _repository.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, errors = new[] { ex.Message } });
        }

        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJson(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Json(new { ok = false, errors = new[] { "Kayıt bulunamadı." } });
        }

        if (entity.IsSystem)
        {
            return Json(new { ok = false, errors = new[] { "Sistem kartları silinemez." } });
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return Json(new { ok = deleted, errors = deleted ? Array.Empty<string>() : new[] { "Silinemedi." } });
    }

    [HttpGet]
    public async Task<IActionResult> GetJson(int id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        return Json(new CardDefinitionEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            PreviewImageUrl = entity.PreviewImageUrl,
            Description = entity.Description,
            IsSystem = entity.IsSystem
        });
    }

    private static string[] FlattenErrors(ModelStateDictionary modelState)
    {
        return modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)).ToArray();
    }
}

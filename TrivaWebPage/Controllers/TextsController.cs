using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class TextsController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageTextBuilderRepository _textBuilderRepository;

    public TextsController(IPage pageRepository, IPageTextBuilderRepository textBuilderRepository)
    {
        _pageRepository = pageRepository;
        _textBuilderRepository = textBuilderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? pageId, CancellationToken cancellationToken)
    {
        var pages = (await _pageRepository.GetAllAsync(cancellationToken))
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PageTabItem
            {
                Id = x.Id,
                Name = string.IsNullOrWhiteSpace(x.Title) ? x.Name : x.Title!
            })
            .ToList();

        int? activePageId = null;
        if (pageId is > 0 && pages.Any(x => x.Id == pageId.Value))
        {
            activePageId = pageId.Value;
        }
        else if (pages.Count > 0)
        {
            activePageId = pages[0].Id;
        }

        TextsEditorPageData? activePage = null;
        if (activePageId is int validPageId)
        {
            activePage = await _textBuilderRepository.GetPageEditorDataAsync(validPageId, cancellationToken);
        }

        return View(new TextsBuilderViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            ActivePage = activePage
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TextsBuilderSaveInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["TextsError"] = "Geçersiz kaydetme isteği.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        List<TextBoxSaveItemInputModel>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<TextBoxSaveItemInputModel>>(
                model.PayloadJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException)
        {
            TempData["TextsError"] = "Metin kutuları okunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items is null)
        {
            TempData["TextsError"] = "Kaydedilecek veri bulunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items.Count > 300)
        {
            TempData["TextsError"] = "Aynı anda en fazla 300 metin kutusu kaydedebilirsiniz.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        await _textBuilderRepository.SavePageTextBoxesAsync(model.PageId, items, cancellationToken);
        TempData["TextsMessage"] = "Metinler kaydedildi.";
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardsController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageCardBuilderRepository _cardBuilderRepository;

    public CardsController(IPage pageRepository, IPageCardBuilderRepository cardBuilderRepository)
    {
        _pageRepository = pageRepository;
        _cardBuilderRepository = cardBuilderRepository;
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

        CardsEditorPageData? activePage = null;
        if (activePageId is int validPageId)
        {
            activePage = await _cardBuilderRepository.GetPageEditorDataAsync(validPageId, cancellationToken);
            if (activePage is not null && activePage.Items.Count == 0)
            {
                activePage.Items = BuildDefaultCards();
            }
        }

        var presets = await _cardBuilderRepository.GetButtonPresetsAsync(cancellationToken);

        return View(new CardsBuilderViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            ActivePage = activePage,
            ButtonPresets = presets
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CardsBuilderSaveInputModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["CardsError"] = "Geçersiz kaydetme isteği.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        List<CardBuilderSaveItemInputModel>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<CardBuilderSaveItemInputModel>>(
                model.PayloadJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            TempData["CardsError"] = "Kart verisi okunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items is null)
        {
            TempData["CardsError"] = "Kaydedilecek kart bulunamadı.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        if (items.Count > 100)
        {
            TempData["CardsError"] = "Aynı sayfada en fazla 100 kart kaydedebilirsiniz.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        await _cardBuilderRepository.SavePageCardsAsync(model.PageId, items, cancellationToken);
        TempData["CardsMessage"] = "Kartlar kaydedildi.";
        return RedirectToAction(nameof(Index), new { pageId = model.PageId });
    }

    private static List<CardEditorItemViewModel> BuildDefaultCards()
    {
        var cards = new List<CardEditorItemViewModel>();
        for (var i = 0; i < 6; i++)
        {
            var hasImage = i < 4;
            cards.Add(new CardEditorItemViewModel
            {
                DisplayOrder = i + 1,
                X = 32 + ((i % 3) * 340),
                Y = 32 + ((i / 3) * 280),
                Width = 300,
                Height = 230,
                IsVisible = true,
                CardDefinitionId = 1,
                Title = $"Kart {i + 1}",
                Subtitle = hasImage ? "Resimli kart" : "Yazı kartı",
                Description = hasImage
                    ? "Bu kartta görsel ve metin birlikte gösterilir."
                    : "Bu kart yalnızca metin içeriğiyle başlar.",
                ShowImage = hasImage,
                ShowButton = true,
                BackgroundColor = "#ffffff",
                TextColor = "#111827",
                BorderColor = "#d1d5db",
                Buttons =
                [
                    new CardEditorButtonViewModel
                    {
                        DisplayOrder = 1,
                        Text = "Detaya Git",
                        BackgroundColor = "#00a19b",
                        TextColor = "#ffffff",
                        BorderColor = "#008f8a",
                        ActionUrl = "https://example.com",
                        ActionTarget = "_self"
                    }
                ]
            });
        }

        return cards;
    }
}

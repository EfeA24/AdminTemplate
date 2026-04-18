using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions.CardOptionAbstractions;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Controllers;

public class CardsController : Controller
{
    private readonly IPage _pageRepository;
    private readonly IPageCardBuilderRepository _cardBuilderRepository;
    private readonly IPageMediaFile _pageMediaFile;
    private readonly IMediaFile _mediaFile;
    private readonly ICardDefinition _cardDefinitionRepository;

    public CardsController(
        IPage pageRepository,
        IPageCardBuilderRepository cardBuilderRepository,
        IPageMediaFile pageMediaFile,
        IMediaFile mediaFile,
        ICardDefinition cardDefinitionRepository)
    {
        _pageRepository = pageRepository;
        _cardBuilderRepository = cardBuilderRepository;
        _pageMediaFile = pageMediaFile;
        _mediaFile = mediaFile;
        _cardDefinitionRepository = cardDefinitionRepository;
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
        IReadOnlyList<PageEditMediaItemViewModel> pageMedia = Array.Empty<PageEditMediaItemViewModel>();

        if (activePageId is int validPageId)
        {
            activePage = await _cardBuilderRepository.GetPageEditorDataAsync(validPageId, cancellationToken);

            var mediaIds = await _pageMediaFile.GetMediaFileIdsByPageAsync(validPageId, cancellationToken);
            var mediaList = new List<PageEditMediaItemViewModel>();
            foreach (var mid in mediaIds)
            {
                var m = await _mediaFile.GetByIdAsync(mid, cancellationToken);
                if (m is null) continue;
                mediaList.Add(new PageEditMediaItemViewModel
                {
                    MediaFileId = m.Id,
                    FilePath = m.FilePath,
                    DisplayName = m.OriginalFileName
                });
            }

            pageMedia = mediaList;
        }

        var definitions = (await _cardDefinitionRepository.GetAllAsync(cancellationToken))
            .OrderBy(d => d.Name)
            .Select(d => new CardDefinitionPresetViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                PreviewImageUrl = d.PreviewImageUrl
            })
            .ToList();

        return View(new CardsBuilderViewModel
        {
            ActivePageId = activePageId,
            Pages = pages,
            ActivePage = activePage,
            ButtonPresets = Array.Empty<CardButtonPresetViewModel>(),
            PageMedia = pageMedia,
            CardDefinitions = definitions
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCardMedia(int pageId, int cardComponentId, int? mediaFileId, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(pageId, cancellationToken);
        if (page is null || page.IsDeleted)
        {
            return NotFound();
        }

        if (mediaFileId is int mid)
        {
            var onPage = await _pageMediaFile.GetMediaFileIdsByPageAsync(pageId, cancellationToken);
            if (!onPage.Contains(mid))
            {
                TempData["CardsError"] = "Bu görsel bu sayfaya yüklenmemiş.";
                return RedirectToAction(nameof(Index), new { pageId });
            }
        }

        await _cardBuilderRepository.UpdateCardComponentMediaAsync(cardComponentId, mediaFileId, cancellationToken);
        TempData["CardsMessage"] = "Kart görseli güncellendi.";
        return RedirectToAction(nameof(Index), new { pageId });
    }
}

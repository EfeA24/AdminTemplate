using Microsoft.AspNetCore.Mvc;
using TrivaWebPage.Abstractions;
using TrivaWebPage.Models;

namespace TrivaWebPage.Controllers;

public class WebsiteSettingsController : Controller
{
    private readonly IGenericRepository<WebsiteSettings> _settingsRepository;
    private const int SingleSettingsId = 1;

    public WebsiteSettingsController(IGenericRepository<WebsiteSettings> settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByIdAsync(SingleSettingsId, cancellationToken);

        if (settings is null)
        {
            settings = new WebsiteSettings
            {
                Id = SingleSettingsId,
                SiteTitle = "Triva",
                PrimaryColor = "#00a19b",
                ContactEmail = string.Empty,
                MaintenanceMode = false
            };

            await _settingsRepository.CreateAsync(settings, cancellationToken);
        }

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WebsiteSettings settings, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(settings);
        }

        settings.Id = SingleSettingsId;
        var updated = await _settingsRepository.UpdateAsync(settings, cancellationToken);

        if (!updated)
        {
            await _settingsRepository.CreateAsync(settings, cancellationToken);
        }

        return RedirectToAction(nameof(Edit));
    }
}


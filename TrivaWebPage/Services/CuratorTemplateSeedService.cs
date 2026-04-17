using Microsoft.EntityFrameworkCore;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Services;

public class CuratorTemplateSeedService
{
    public const string TemplateCode = "triva";

    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public CuratorTemplateSeedService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await RemoveLegacyCuratorTemplateAsync(cancellationToken);

        var template = await _dbContext.PageTemplates.FirstOrDefaultAsync(x => x.Code == TemplateCode, cancellationToken);
        if (template is null)
        {
            template = new PageTemplate
            {
                Name = "Triva",
                Code = TemplateCode,
                Description = "İki ana sayfa ve iki menü sayfası şablonları.",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _dbContext.PageTemplates.Add(template);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            template.Name = "Triva";
            template.Description = "İki ana sayfa ve iki menü sayfası şablonları.";
            template.IsActive = true;
        }

        var fileMap = new (string Name, int DisplayOrder, string FileName, string? PreviewImagePath)[]
        {
            ("Ana Sayfa 1", 0, "ana-sayfa-1.html", "/pictures/template-previews/triva-ana-sayfa-1-preview.png"),
            ("Ana Sayfa 2", 1, "ana-sayfa-2.html", "/pictures/template-previews/triva-ana-sayfa-2-preview.png"),
            ("Menü Sayfası 1", 2, "menu-1.html", "/pictures/template-previews/triva-menu-1-preview.png"),
            ("Menü Sayfası 2", 3, "menu-2.html", "/pictures/template-previews/triva-menu-2-preview.png")
        };

        var seedRoot = Path.Combine(_environment.ContentRootPath, "SeedData", "Triva");
        var validNames = fileMap.Select(f => f.Name).ToList();

        foreach (var (name, displayOrder, fileName, previewImagePath) in fileMap)
        {
            var fullPath = Path.Combine(seedRoot, fileName);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var html = await File.ReadAllTextAsync(fullPath, cancellationToken);
            var row = await _dbContext.PageTemplatePages
                .FirstOrDefaultAsync(x => x.PageTemplateId == template.Id && x.Name == name, cancellationToken);

            if (row is null)
            {
                _dbContext.PageTemplatePages.Add(new PageTemplatePage
                {
                    PageTemplateId = template.Id,
                    Name = name,
                    DisplayOrder = displayOrder,
                    PreviewImagePath = previewImagePath,
                    HtmlContent = html
                });
            }
            else
            {
                row.DisplayOrder = displayOrder;
                row.PreviewImagePath = previewImagePath;
                row.HtmlContent = html;
            }
        }

        var orphans = await _dbContext.PageTemplatePages
            .Where(x => x.PageTemplateId == template.Id && !validNames.Contains(x.Name))
            .ToListAsync(cancellationToken);
        if (orphans.Count > 0)
        {
            _dbContext.PageTemplatePages.RemoveRange(orphans);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RemoveLegacyCuratorTemplateAsync(CancellationToken cancellationToken)
    {
        var legacy = await _dbContext.PageTemplates.FirstOrDefaultAsync(x => x.Code == "curator", cancellationToken);
        if (legacy is null)
        {
            return;
        }

        var legacyPages = await _dbContext.PageTemplatePages
            .Where(x => x.PageTemplateId == legacy.Id)
            .ToListAsync(cancellationToken);
        _dbContext.PageTemplatePages.RemoveRange(legacyPages);
        _dbContext.PageTemplates.Remove(legacy);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

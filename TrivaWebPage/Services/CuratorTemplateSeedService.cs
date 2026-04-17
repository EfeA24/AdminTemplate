using Microsoft.EntityFrameworkCore;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Services;

public class CuratorTemplateSeedService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public CuratorTemplateSeedService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.PageTemplates.FirstOrDefaultAsync(x => x.Code == "curator", cancellationToken);
        if (template is null)
        {
            template = new PageTemplate
            {
                Name = "The Curator",
                Code = "curator",
                Description = "Creative and dark variants for main and gallery pages.",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _dbContext.PageTemplates.Add(template);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var fileMap = new (string Name, int DisplayOrder, string FileName, string PreviewImagePath)[]
        {
            ("Creative Main", 0, "creative-main.html", "/pictures/template-previews/creative-main.png"),
            ("Creative Gallery", 1, "creative-gallery.html", "/pictures/template-previews/creative-gallery.png"),
            ("Dark Main", 2, "dark-main.html", "/pictures/template-previews/dark-main.png"),
            ("Dark Gallery", 3, "dark-gallery.html", "/pictures/template-previews/dark-gallery.png")
        };

        var seedRoot = Path.Combine(_environment.ContentRootPath, "SeedData", "Curator");
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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

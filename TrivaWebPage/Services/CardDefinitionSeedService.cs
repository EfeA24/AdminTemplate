using Microsoft.EntityFrameworkCore;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.CardOptions;

namespace TrivaWebPage.Services;

/// <summary>Ensures default system card templates exist (idempotent).</summary>
public class CardDefinitionSeedService
{
    public const string PlaceholderPreviewUrl = "/pictures/placeholder-card.svg";

    private readonly AppDbContext _dbContext;

    public CardDefinitionSeedService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var defaults = new (string Code, string Name)[]
        {
            ("info", "Bilgi Kartı"),
            ("cta", "Aksiyon Kartı"),
            ("image", "Görselli Kart"),
            ("product-card", "Ürün kartı")
        };

        var utc = DateTime.UtcNow;

        foreach (var (code, name) in defaults)
        {
            var row = await _dbContext.CardDefinitions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            if (row is null)
            {
                _dbContext.CardDefinitions.Add(new CardDefinition
                {
                    Name = name,
                    Code = code,
                    PreviewImageUrl = PlaceholderPreviewUrl,
                    Description = null,
                    IsSystem = true,
                    CreatedDate = utc,
                    UpdatedDate = utc
                });
            }
            else
            {
                row.Name = name;
                row.IsSystem = true;
                if (string.IsNullOrWhiteSpace(row.PreviewImageUrl))
                {
                    row.PreviewImageUrl = PlaceholderPreviewUrl;
                }

                row.UpdatedDate = utc;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

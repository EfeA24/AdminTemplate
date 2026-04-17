using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class ColorPaletteRepository : GenericRepository<ColorPalette>, IColorPalette
{
    public ColorPaletteRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "ColorPalettes")
    {
    }
}

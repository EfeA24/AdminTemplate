namespace TrivaWebPage.Models.General;

public class ColorPalette
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string PrimaryHex { get; set; } = null!;
    public string SecondaryHex { get; set; } = null!;
    public string MutedHex { get; set; } = null!;
    public string AccentHex { get; set; } = null!;

    public ICollection<Page> Pages { get; set; } = new List<Page>();
}

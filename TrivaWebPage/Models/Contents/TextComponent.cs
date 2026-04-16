using TrivaWebPage.Models.General;

namespace TrivaWebPage.Models.Contents
{
    public class TextComponent
    {
        public int Id { get; set; }

        public int PageComponentId { get; set; }
        public PageComponent PageComponent { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string? FontFamily { get; set; }
        public int FontSize { get; set; }
        public string? FontWeight { get; set; }
        public string? TextColor { get; set; }
        public string? TextAlign { get; set; }

        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
    }
}

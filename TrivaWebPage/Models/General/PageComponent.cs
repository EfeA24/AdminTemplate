using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.General
{
    public class PageComponent
    {
        public int Id { get; set; }

        public int PageSectionId { get; set; }
        public PageSection PageSection { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string ComponentType { get; set; } = null!; // Text, Image, Card, Button

        public int DisplayOrder { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string? CssClass { get; set; }
        public string? InlineStyle { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public TextComponent? TextComponent { get; set; }
        public ImageComponent? ImageComponent { get; set; }
        public ButtonComponent? ButtonComponent { get; set; }
        public CardComponent? CardComponent { get; set; }
    }
}

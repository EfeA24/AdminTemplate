using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Models.Contents
{
    public class CardComponent
    {
        public int Id { get; set; }

        public int PageComponentId { get; set; }
        public PageComponent PageComponent { get; set; } = null!;

        public int CardDefinitionId { get; set; }
        public CardDefinition CardDefinition { get; set; } = null!;

        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }

        /// <summary>Optional raw HTML for custom cards (Cards builder); FK uses a normal definition (e.g. info).</summary>
        public string? HtmlFragment { get; set; }

        public int? MediaFileId { get; set; }
        public MediaFile? MediaFile { get; set; }

        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public string? BorderColor { get; set; }

        public bool ShowImage { get; set; }
        public bool ShowButton { get; set; }

        public ICollection<CardFieldValue> FieldValues { get; set; } = new List<CardFieldValue>();
        public ICollection<CardButton> Buttons { get; set; } = new List<CardButton>();
    }
}

using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.CardOptions
{
    public class CardButton
    {
        public int Id { get; set; }

        public int CardComponentId { get; set; }
        public CardComponent CardComponent { get; set; } = null!;

        public string Text { get; set; } = null!;
        public string? Icon { get; set; }

        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public string? BorderColor { get; set; }

        public int DisplayOrder { get; set; }

        public int? ActionDefinitionId { get; set; }
        public ActionDefinition? ActionDefinition { get; set; }
    }
}

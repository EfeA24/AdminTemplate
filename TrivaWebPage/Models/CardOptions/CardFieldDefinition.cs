namespace TrivaWebPage.Models.CardOptions
{
    public class CardFieldDefinition
    {
        public int Id { get; set; }

        public int CardDefinitionId { get; set; }
        public CardDefinition CardDefinition { get; set; } = null!;

        public string FieldName { get; set; } = null!;   // Title, Price, BadgeText, TagLine
        public string FieldKey { get; set; } = null!;    // title, price, badgeText
        public string FieldType { get; set; } = null!;   // Text, TextArea, Number, Color, Image, Url
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }
}

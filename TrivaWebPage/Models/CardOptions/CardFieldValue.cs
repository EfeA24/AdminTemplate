using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.CardOptions
{
    public class CardFieldValue
    {
        public int Id { get; set; }

        public int CardComponentId { get; set; }
        public CardComponent CardComponent { get; set; } = null!;

        public int CardFieldDefinitionId { get; set; }
        public CardFieldDefinition CardFieldDefinition { get; set; } = null!;

        public string? FieldValue { get; set; }
    }
}

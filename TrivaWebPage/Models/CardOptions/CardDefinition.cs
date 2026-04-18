using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.CardOptions
{
    /// <summary>Reusable UI template metadata (no stored HTML).</summary>
    public class CardDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string PreviewImageUrl { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsSystem { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public ICollection<CardFieldDefinition> FieldDefinitions { get; set; } = new List<CardFieldDefinition>();
        public ICollection<CardComponent> CardComponents { get; set; } = new List<CardComponent>();
    }
}

using TrivaWebPage.Models.Contents;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Models.CardOptions
{
    public class CardDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string CardType { get; set; } = null!; // Hero, Banner, Info, Product, ImageTop, ImageLeft
        public string? Description { get; set; }

        public int? PreviewMediaFileId { get; set; }
        public MediaFile? PreviewMediaFile { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<CardFieldDefinition> FieldDefinitions { get; set; } = new List<CardFieldDefinition>();
        public ICollection<CardComponent> CardComponents { get; set; } = new List<CardComponent>();
    }
}

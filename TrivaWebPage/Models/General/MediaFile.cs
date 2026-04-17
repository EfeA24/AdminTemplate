using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.General
{
    public class MediaFile
    {
        public int Id { get; set; }

        public string FileName { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? AltText { get; set; }

        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? FileExtension { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }

        public DateTime UploadedDate { get; set; }

        public ICollection<ImageComponent> ImageComponents { get; set; } = new List<ImageComponent>();
        public ICollection<CardComponent> CardComponents { get; set; } = new List<CardComponent>();
        public ICollection<CardDefinition> CardDefinitions { get; set; } = new List<CardDefinition>();
        public ICollection<PageMediaFile> PageMediaFiles { get; set; } = new List<PageMediaFile>();
    }
}

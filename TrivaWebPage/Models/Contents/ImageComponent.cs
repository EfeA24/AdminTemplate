using TrivaWebPage.Models.General;

namespace TrivaWebPage.Models.Contents
{
    public class ImageComponent
    {
        public int Id { get; set; }

        public int PageComponentId { get; set; }
        public PageComponent PageComponent { get; set; } = null!;

        public int MediaFileId { get; set; }
        public MediaFile MediaFile { get; set; } = null!;

        public string? AltText { get; set; }
        public string? FitType { get; set; } // Cover, Contain, Fill
        public int? BorderRadius { get; set; }
        public bool HasShadow { get; set; }
    }
}

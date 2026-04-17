namespace TrivaWebPage.Models.General
{
    public class PageMediaFile
    {
        public int Id { get; set; }

        public int PageId { get; set; }
        public int MediaFileId { get; set; }

        public Page Page { get; set; } = null!;
        public MediaFile MediaFile { get; set; } = null!;
    }
}

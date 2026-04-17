namespace TrivaWebPage.Models.General
{
    public class Page
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsHomePage { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
        public ICollection<PageMediaFile> PageMediaFiles { get; set; } = new List<PageMediaFile>();
    }
}

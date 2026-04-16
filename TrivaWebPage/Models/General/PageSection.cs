namespace TrivaWebPage.Models.General
{
    public class PageSection
    {
        public int Id { get; set; }

        public int PageId { get; set; }
        public Page Page { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string SectionType { get; set; } = null!; // Header, Content, Footer, Custom

        public int DisplayOrder { get; set; }

        public string? CssClass { get; set; }
        public string? InlineStyle { get; set; }

        public bool IsVisible { get; set; } = true;

        public ICollection<PageComponent> Components { get; set; } = new List<PageComponent>();
    }
}

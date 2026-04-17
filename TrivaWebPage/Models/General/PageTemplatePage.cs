namespace TrivaWebPage.Models.General;

public class PageTemplatePage
{
    public int Id { get; set; }

    public int PageTemplateId { get; set; }
    public PageTemplate PageTemplate { get; set; } = null!;

    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public string HtmlContent { get; set; } = null!;

    public ICollection<Page> Pages { get; set; } = new List<Page>();
}

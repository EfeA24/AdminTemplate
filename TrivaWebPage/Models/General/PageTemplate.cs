namespace TrivaWebPage.Models.General;

public class PageTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public ICollection<PageTemplatePage> Pages { get; set; } = new List<PageTemplatePage>();
}

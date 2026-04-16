namespace TrivaWebPage.Models;

/// <summary>
/// Single-operator website configuration record.
/// Convention mapping expects table name: WebsiteSettings and key: Id.
/// For single-user admin scenarios, keep one row (for example Id = 1).
/// </summary>
public class WebsiteSettings
{
    public int Id { get; set; }

    public string SiteTitle { get; set; } = string.Empty;

    public string PrimaryColor { get; set; } = "#00a19b";

    public string ContactEmail { get; set; } = string.Empty;

    public bool MaintenanceMode { get; set; }
}


using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Models.Contents
{
    public class ButtonComponent
    {
        public int Id { get; set; }

        public int PageComponentId { get; set; }
        public PageComponent PageComponent { get; set; } = null!;

        public string Text { get; set; } = null!;
        public string? Icon { get; set; }

        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public string? BorderColor { get; set; }

        public string? SizeType { get; set; }   // Small, Medium, Large
        public string? StyleType { get; set; }  // Filled, Outline, Ghost

        public int? ActionDefinitionId { get; set; }
        public ActionDefinition? ActionDefinition { get; set; }
    }
}

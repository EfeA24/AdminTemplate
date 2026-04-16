using TrivaWebPage.Models.Contents;

namespace TrivaWebPage.Models.CardOptions
{
    public class ActionDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string ActionType { get; set; } = null!; // Link, Function, Both

        public string? Url { get; set; }
        public string? Target { get; set; } // _self, _blank

        public string? FunctionName { get; set; }
        public string? ParametersJson { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ButtonComponent> ButtonComponents { get; set; } = new List<ButtonComponent>();
        public ICollection<CardButton> CardButtons { get; set; } = new List<CardButton>();
    }
}

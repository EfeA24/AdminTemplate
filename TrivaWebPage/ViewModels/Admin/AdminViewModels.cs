using System.ComponentModel.DataAnnotations;

namespace TrivaWebPage.ViewModels.Admin;

public class PageEditViewModel
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    [Range(1, 5000)] public int Width { get; set; } = 1920;
    [Range(1, 5000)] public int Height { get; set; } = 1080;
    public bool IsHomePage { get; set; }
    public bool IsPublished { get; set; }
    public bool IsDeleted { get; set; }
    public int DisplayOrder { get; set; }
}

public class PageSectionEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string SectionType { get; set; } = "Content";
    public int DisplayOrder { get; set; }
    public string? CssClass { get; set; }
    public string? InlineStyle { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class PageComponentEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageSectionId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string ComponentType { get; set; } = "Text";
    public int DisplayOrder { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    [Range(1, 5000)] public int Width { get; set; } = 300;
    [Range(1, 5000)] public int Height { get; set; } = 200;
    public string? CssClass { get; set; }
    public string? InlineStyle { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class TextComponentEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageComponentId { get; set; }
    [Required] public string Content { get; set; } = string.Empty;
    public string? FontFamily { get; set; }
    [Range(1, 200)] public int FontSize { get; set; } = 16;
    public string? FontWeight { get; set; }
    public string? TextColor { get; set; }
    public string? TextAlign { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
}

public class ImageComponentEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageComponentId { get; set; }
    [Required] public int MediaFileId { get; set; }
    public string? AltText { get; set; }
    public string? FitType { get; set; }
    public int? BorderRadius { get; set; }
    public bool HasShadow { get; set; }
}

public class ButtonComponentEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageComponentId { get; set; }
    [Required] public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public string? SizeType { get; set; }
    public string? StyleType { get; set; }
    public int? ActionDefinitionId { get; set; }
}

public class CardComponentEditViewModel
{
    public int Id { get; set; }
    [Required] public int PageComponentId { get; set; }
    [Required] public int CardDefinitionId { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public int? MediaFileId { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public bool ShowImage { get; set; }
    public bool ShowButton { get; set; }
}

public class ActionDefinitionEditViewModel
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string ActionType { get; set; } = "Link";
    public string? Url { get; set; }
    public string? Target { get; set; }
    public string? FunctionName { get; set; }
    public string? ParametersJson { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CardDefinitionEditViewModel
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Code { get; set; } = string.Empty;
    [Required] public string CardType { get; set; } = "Info";
    public string? Description { get; set; }
    public int? PreviewMediaFileId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CardFieldDefinitionEditViewModel
{
    public int Id { get; set; }
    [Required] public int CardDefinitionId { get; set; }
    [Required] public string FieldName { get; set; } = string.Empty;
    [Required] public string FieldKey { get; set; } = string.Empty;
    [Required] public string FieldType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}

public class CardFieldValueEditViewModel
{
    public int Id { get; set; }
    [Required] public int CardComponentId { get; set; }
    [Required] public int CardFieldDefinitionId { get; set; }
    public string? FieldValue { get; set; }
}

public class CardButtonEditViewModel
{
    public int Id { get; set; }
    [Required] public int CardComponentId { get; set; }
    [Required] public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public int DisplayOrder { get; set; }
    public int? ActionDefinitionId { get; set; }
}

public class MediaFileEditViewModel
{
    public int Id { get; set; }
    [Required] public string FileName { get; set; } = string.Empty;
    [Required] public string OriginalFileName { get; set; } = string.Empty;
    [Required] public string FilePath { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? FileExtension { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public class LoginViewModel
{
    [Required] public string UserName { get; set; } = "admin";
    [Required] public string Password { get; set; } = "12345";
    public bool RememberMe { get; set; }
}

public class UserEditViewModel
{
    public int Id { get; set; }
    [Required] public string UserName { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class ImagesGalleryViewModel
{
    public const string TabAll = "all";

    public string ActiveTab { get; set; } = TabAll;
    public int? ActivePageId { get; set; }
    public IReadOnlyList<PageTabItem> Pages { get; init; } = Array.Empty<PageTabItem>();
    public IReadOnlyList<MediaFileCardItem> Items { get; init; } = Array.Empty<MediaFileCardItem>();
    public IReadOnlyDictionary<int, IReadOnlyList<int>> AssignmentsByMediaFileId { get; init; } =
        new Dictionary<int, IReadOnlyList<int>>();
}

public class PageTabItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class MediaFileCardItem
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
}

public class ImageAssignInputModel
{
    [Range(1, int.MaxValue)]
    public int MediaFileId { get; set; }

    public int[]? PageIds { get; set; }
}

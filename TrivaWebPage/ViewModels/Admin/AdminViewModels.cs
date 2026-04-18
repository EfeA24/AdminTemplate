using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TrivaWebPage.ViewModels.Admin;

public class PageEditViewModel
{
    public int Id { get; set; }

    [Display(Name = "Ad")]
    [Required(ErrorMessage = "Ad zorunludur.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kısa adres")]
    [Required(ErrorMessage = "Kısa adres zorunludur.")]
    [RegularExpression(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Kısa adres yalnızca küçük harf, rakam ve tire içerebilir; tire ile başlayıp bitemez.")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Başlık")]
    public string? Title { get; set; }

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Genişlik")]
    [Range(1, 5000, ErrorMessage = "Genişlik 1 ile 5000 arasında olmalıdır.")]
    public int Width { get; set; } = 1920;

    [Display(Name = "Yükseklik")]
    [Range(1, 5000, ErrorMessage = "Yükseklik 1 ile 5000 arasında olmalıdır.")]
    public int Height { get; set; } = 1080;

    [Display(Name = "Ana sayfa")]
    public bool IsHomePage { get; set; }

    [Display(Name = "Yayında")]
    public bool IsPublished { get; set; } = true;

    [Display(Name = "Silinmiş")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Görüntüleme sırası")]
    public int DisplayOrder { get; set; }

    /// <summary>UI: şablon seçimi (iç sayfa listesini filtrelemek için).</summary>
    [Display(Name = "Şablon")]
    public int? PageTemplateId { get; set; }

    [Display(Name = "Şablon içi sayfa")]
    public int? PageTemplatePageId { get; set; }

    [Display(Name = "Renk paleti")]
    public int? ColorPaletteId { get; set; }
}

public class PageTemplatePageLineViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public string? HtmlContent { get; set; }

    public string? PreviewImagePath { get; set; }
}

public class PageTemplateEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>Tek sayfa veya ZIP sonrası düzenleme için ana HTML (ZIP yoksa zorunlu).</summary>
    public string? MasterHtml { get; set; }

    /// <summary>Sunucu tarafında ZIP veya MasterHtml ile doldurulur.</summary>
    public List<PageTemplatePageLineViewModel> Pages { get; set; } = new();
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
    [Required] public string PreviewImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}

public class CardDefinitionPresetViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string PreviewImageUrl { get; init; } = string.Empty;
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

public class PageEditIndexViewModel
{
    public int? ActivePageId { get; set; }
    public IReadOnlyList<PageTabItem> Pages { get; init; } = Array.Empty<PageTabItem>();
    public PageEditCanvasPageData? ActivePage { get; set; }
    public string[] FontOptions { get; init; } = ["Inter", "Manrope", "Arial", "Verdana", "Tahoma", "Times New Roman", "Georgia"];
    /// <summary>Page-scoped media for sidebar (active page).</summary>
    public IReadOnlyList<PageEditMediaItemViewModel> PageMedia { get; init; } = Array.Empty<PageEditMediaItemViewModel>();
    /// <summary>Card components on the active page (read-only preview in PageEdit).</summary>
    public IReadOnlyList<PageEditCardPreviewItemViewModel> Cards { get; init; } = Array.Empty<PageEditCardPreviewItemViewModel>();
}

public class PageEditMediaItemViewModel
{
    public int MediaFileId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public class PageEditCardPreviewItemViewModel
{
    public int ComponentId { get; init; }
    public int CardComponentId { get; init; }
    public string? Title { get; init; }
    public int? MediaFileId { get; init; }
    public string? MediaFilePath { get; init; }
    public bool ShowImage { get; init; }
    public string? HtmlFragment { get; init; }
}

public class PageEditCanvasPageData
{
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
    public int? TemplatePageId { get; set; }
    public string? TemplatePageName { get; set; }
    public string? TemplateHtml { get; set; }
    public List<TextBoxEditorItemViewModel> Items { get; set; } = new();
}

public class TextBoxEditorItemViewModel
{
    public int ComponentId { get; set; }
    public int DisplayOrder { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FontFamily { get; set; }
    public int FontSize { get; set; }
    public string? FontWeight { get; set; }
    public string? TextColor { get; set; }
    public string? TextAlign { get; set; }
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
}

public class PageEditSaveInputModel
{
    [Range(1, int.MaxValue)]
    public int PageId { get; set; }

    [Required]
    public string PayloadJson { get; set; } = "[]";

    /// <summary>İsteğe bağlı: şablondaki img değişiklikleri sonrası tam HTML (sanitize edilir).</summary>
    public string? RenderedHtmlOverride { get; set; }
}

public record TextBoxSaveItemInputModel
{
    public int ComponentId { get; init; }
    public int DisplayOrder { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public bool IsVisible { get; init; } = true;
    public string Content { get; init; } = string.Empty;
    public string? FontFamily { get; init; }
    public int FontSize { get; init; } = 16;
    public string? FontWeight { get; init; }
    public string? TextColor { get; init; }
    public string? TextAlign { get; init; }
    public bool IsBold { get; init; }
    public bool IsItalic { get; init; }
    public bool IsUnderline { get; init; }
}

public class CardsBuilderViewModel
{
    public int? ActivePageId { get; set; }
    public IReadOnlyList<PageTabItem> Pages { get; init; } = Array.Empty<PageTabItem>();
    public CardsEditorPageData? ActivePage { get; set; }
    public IReadOnlyList<CardButtonPresetViewModel> ButtonPresets { get; init; } = Array.Empty<CardButtonPresetViewModel>();
    public IReadOnlyList<PageEditMediaItemViewModel> PageMedia { get; init; } = Array.Empty<PageEditMediaItemViewModel>();
    public IReadOnlyList<CardDefinitionPresetViewModel> CardDefinitions { get; init; } = Array.Empty<CardDefinitionPresetViewModel>();
}

public class CardsEditorPageData
{
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
    public int? TemplatePageId { get; set; }
    public string? TemplatePageName { get; set; }
    public string? TemplateHtml { get; set; }
    public List<CardEditorItemViewModel> Items { get; set; } = new();
}

public class CardEditorItemViewModel
{
    public int ComponentId { get; set; }
    public int CardComponentId { get; set; }
    public int DisplayOrder { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public int? CardDefinitionId { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? HtmlFragment { get; set; }
    public int? MediaFileId { get; set; }
    public string? MediaFilePath { get; set; }
    public bool ShowImage { get; set; }
    public bool ShowButton { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public List<CardEditorButtonViewModel> Buttons { get; set; } = new();
}

public class CardEditorButtonViewModel
{
    public int Id { get; set; }
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
    public int? ActionDefinitionId { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionTarget { get; set; }
}

public class CardButtonPresetViewModel
{
    public int PresetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public string? TextColor { get; set; }
    public string? BorderColor { get; set; }
}

public class CardsBuilderSaveInputModel
{
    [Range(1, int.MaxValue)]
    public int PageId { get; set; }

    [Required]
    public string PayloadJson { get; set; } = "[]";
}

public record CardBuilderSaveItemInputModel
{
    public int ComponentId { get; init; }
    public int CardComponentId { get; init; }
    public int DisplayOrder { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; } = 320;
    public int Height { get; init; } = 240;
    public bool IsVisible { get; init; } = true;
    public int? CardDefinitionId { get; init; }
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? HtmlFragment { get; init; }
    public int? MediaFileId { get; init; }
    public bool ShowImage { get; init; }
    public bool ShowButton { get; init; } = true;
    public string? BackgroundColor { get; init; }
    public string? TextColor { get; init; }
    public string? BorderColor { get; init; }
    public List<CardBuilderSaveButtonInputModel> Buttons { get; init; } = new();
}

public record CardBuilderSaveButtonInputModel
{
    public int Id { get; init; }
    public int DisplayOrder { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public string? BackgroundColor { get; init; }
    public string? TextColor { get; init; }
    public string? BorderColor { get; init; }
    public int? ActionDefinitionId { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionTarget { get; init; }
}

public class TemplateCanvasViewModel
{
    public int? ActivePageId { get; set; }
    public IReadOnlyList<PageTabItem> Pages { get; init; } = Array.Empty<PageTabItem>();
    public IReadOnlyList<TemplateCanvasPaletteData> AvailablePalettes { get; init; } = Array.Empty<TemplateCanvasPaletteData>();
    public TemplateCanvasPageData? ActivePage { get; set; }
    /// <summary>Page-scoped media for Template Canvas modal (active page).</summary>
    public IReadOnlyList<PageEditMediaItemViewModel> PageMedia { get; init; } = Array.Empty<PageEditMediaItemViewModel>();
    /// <summary>All uploaded images (Resimler / kütüphane) for the library picker modal.</summary>
    public IReadOnlyList<PageEditMediaItemViewModel> LibraryMedia { get; init; } = Array.Empty<PageEditMediaItemViewModel>();
}

public class TemplateCanvasPageData
{
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
    public string? TemplatePageName { get; set; }
    public string? HtmlContent { get; set; }
    public TemplateCanvasPaletteData? Palette { get; set; }
}

public class TemplateCanvasPaletteData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryHex { get; set; } = "#111111";
    public string SecondaryHex { get; set; } = "#333333";
    public string MutedHex { get; set; } = "#777777";
    public string AccentHex { get; set; } = "#00a19b";
}

public class TemplateCanvasSaveInputModel
{
    [Range(1, int.MaxValue)]
    public int PageId { get; set; }

    public int? ColorPaletteId { get; set; }

    [Required]
    public string HtmlContent { get; set; } = string.Empty;
}

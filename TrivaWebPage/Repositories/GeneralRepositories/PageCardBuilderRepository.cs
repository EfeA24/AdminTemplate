using System.Text.RegularExpressions;
using Dapper;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class PageCardBuilderRepository : IPageCardBuilderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PageCardBuilderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CardsEditorPageData?> GetPageEditorDataAsync(int pageId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT TOP (1)
                               p.[Id] AS [PageId],
                               p.[Name] AS [PageName],
                               p.[Width] AS [PageWidth],
                               p.[Height] AS [PageHeight],
                               p.[PageTemplatePageId] AS [TemplatePageId],
                               pt.[Name] AS [TemplatePageName],
                               COALESCE(p.[RenderedHtmlOverride], pt.[HtmlContent]) AS [TemplateHtml]
                           FROM [Pages] p
                           LEFT JOIN [PageTemplatePages] pt ON pt.[Id] = p.[PageTemplatePageId]
                           WHERE p.[Id] = @PageId AND p.[IsDeleted] = 0;

                           SELECT
                               pc.[Id] AS [ComponentId],
                               cc.[Id] AS [CardComponentId],
                               pc.[DisplayOrder],
                               pc.[X],
                               pc.[Y],
                               pc.[Width],
                               pc.[Height],
                               pc.[IsVisible],
                               cc.[CardDefinitionId],
                               cc.[Title],
                               cc.[Subtitle],
                               cc.[Description],
                               cc.[HtmlFragment],
                               cc.[MediaFileId],
                               mf.[FilePath] AS [MediaFilePath],
                               cc.[ShowImage],
                               cc.[ShowButton],
                               cc.[BackgroundColor],
                               cc.[TextColor],
                               cc.[BorderColor]
                           FROM [PageSections] ps
                           INNER JOIN [PageComponents] pc ON pc.[PageSectionId] = ps.[Id] AND pc.[ComponentType] = 'Card'
                           INNER JOIN [CardComponents] cc ON cc.[PageComponentId] = pc.[Id]
                           LEFT JOIN [MediaFiles] mf ON mf.[Id] = cc.[MediaFileId]
                           WHERE ps.[PageId] = @PageId
                           ORDER BY pc.[DisplayOrder], pc.[Id];

                           SELECT
                               cb.[Id],
                               cb.[CardComponentId],
                               cb.[DisplayOrder],
                               cb.[Text],
                               cb.[Icon],
                               cb.[BackgroundColor],
                               cb.[TextColor],
                               cb.[BorderColor],
                               cb.[ActionDefinitionId],
                               ad.[Url] AS [ActionUrl],
                               ad.[Target] AS [ActionTarget]
                           FROM [PageSections] ps
                           INNER JOIN [PageComponents] pc ON pc.[PageSectionId] = ps.[Id] AND pc.[ComponentType] = 'Card'
                           INNER JOIN [CardComponents] cc ON cc.[PageComponentId] = pc.[Id]
                           INNER JOIN [CardButtons] cb ON cb.[CardComponentId] = cc.[Id]
                           LEFT JOIN [ActionDefinitions] ad ON ad.[Id] = cb.[ActionDefinitionId]
                           WHERE ps.[PageId] = @PageId
                           ORDER BY cb.[CardComponentId], cb.[DisplayOrder], cb.[Id];
                           """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { PageId = pageId }, cancellationToken: cancellationToken));

        var page = await multi.ReadSingleOrDefaultAsync<CardsEditorPageData>();
        if (page is null)
        {
            return null;
        }

        var cards = (await multi.ReadAsync<CardEditorItemViewModel>()).ToList();
        var buttons = (await multi.ReadAsync<CardButtonRow>()).ToList();
        var buttonsByCard = buttons
            .GroupBy(x => x.CardComponentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new CardEditorButtonViewModel
                {
                    Id = x.Id,
                    DisplayOrder = x.DisplayOrder,
                    Text = x.Text,
                    Icon = x.Icon,
                    BackgroundColor = x.BackgroundColor,
                    TextColor = x.TextColor,
                    BorderColor = x.BorderColor,
                    ActionDefinitionId = x.ActionDefinitionId,
                    ActionUrl = x.ActionUrl,
                    ActionTarget = x.ActionTarget
                }).ToList());

        foreach (var card in cards)
        {
            if (buttonsByCard.TryGetValue(card.CardComponentId, out var cardButtons))
            {
                card.Buttons = cardButtons;
            }
        }

        page.Items = cards;
        return page;
    }

    public Task<IReadOnlyList<CardButtonPresetViewModel>> GetButtonPresetsAsync(CancellationToken cancellationToken = default)
    {
        var presets = new List<CardButtonPresetViewModel>
        {
            new() { PresetId = 1, Name = "Detay", Text = "Detaya Git", BackgroundColor = "#00a19b", TextColor = "#ffffff", BorderColor = "#008f8a", UrlPlaceholder = "https://example.com/detay", Target = "_self" },
            new() { PresetId = 2, Name = "Keşfet", Text = "Keşfet", BackgroundColor = "#fb3640", TextColor = "#ffffff", BorderColor = "#d12a33", UrlPlaceholder = "https://example.com/kesfet", Target = "_blank" },
            new() { PresetId = 3, Name = "İncele", Text = "İncele", BackgroundColor = "#111827", TextColor = "#ffffff", BorderColor = "#111827", UrlPlaceholder = "https://example.com/incele", Target = "_blank" },
            new() { PresetId = 4, Name = "Oku", Text = "Devamını Oku", BackgroundColor = "#ffffff", TextColor = "#111827", BorderColor = "#d1d5db", UrlPlaceholder = "https://example.com/oku", Target = "_self" },
            new() { PresetId = 5, Name = "Demo", Text = "Demo Aç", BackgroundColor = "#4f46e5", TextColor = "#ffffff", BorderColor = "#4338ca", UrlPlaceholder = "https://example.com/demo", Target = "_blank" },
            new() { PresetId = 6, Name = "Kaydol", Text = "Hemen Kaydol", BackgroundColor = "#16a34a", TextColor = "#ffffff", BorderColor = "#15803d", UrlPlaceholder = "https://example.com/kaydol", Target = "_self" },
            new() { PresetId = 7, Name = "Teklif", Text = "Teklif Al", BackgroundColor = "#f59e0b", TextColor = "#111827", BorderColor = "#d97706", UrlPlaceholder = "https://example.com/teklif", Target = "_self" },
            new() { PresetId = 8, Name = "Destek", Text = "Destek Talebi", BackgroundColor = "#0ea5e9", TextColor = "#ffffff", BorderColor = "#0284c7", UrlPlaceholder = "https://example.com/destek", Target = "_self" },
            new() { PresetId = 9, Name = "İletişim", Text = "İletişime Geç", BackgroundColor = "#ec4899", TextColor = "#ffffff", BorderColor = "#db2777", UrlPlaceholder = "https://example.com/iletisim", Target = "_self" },
            new() { PresetId = 10, Name = "Döküman", Text = "Döküman İndir", BackgroundColor = "#374151", TextColor = "#ffffff", BorderColor = "#1f2937", UrlPlaceholder = "https://example.com/dokuman", Target = "_blank" }
        };

        return Task.FromResult<IReadOnlyList<CardButtonPresetViewModel>>(presets);
    }

    public async Task SavePageCardsAsync(int pageId, IReadOnlyList<CardBuilderSaveItemInputModel> items, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        using var tx = connection.BeginTransaction();

        try
        {
            var page = await connection.QuerySingleOrDefaultAsync<PageRow>(
                new CommandDefinition(
                    "SELECT TOP (1) [Id], [Name], [Width], [Height] FROM [Pages] WHERE [Id] = @PageId AND [IsDeleted] = 0;",
                    new { PageId = pageId },
                    tx,
                    cancellationToken: cancellationToken));

            if (page is null)
            {
                throw new InvalidOperationException("Kaydedilecek sayfa bulunamadı.");
            }

            var sectionId = await EnsureSectionAsync(connection, tx, pageId, cancellationToken);
            var htmlCardDefinitionId = await connection.QuerySingleOrDefaultAsync<int?>(
                new CommandDefinition(
                    "SELECT TOP (1) [Id] FROM [CardDefinitions] WHERE [Code] = @Code;",
                    new { Code = "info" },
                    tx,
                    cancellationToken: cancellationToken));
            if (!htmlCardDefinitionId.HasValue)
            {
                throw new InvalidOperationException("CardDefinitions içinde 'info' kodu bulunamadı. Migration uygulandığından emin olun.");
            }

            var incoming = items
                .Select((item, idx) => Normalize(item, page.Width, page.Height, idx + 1, htmlCardDefinitionId.Value))
                .ToList();

            var existing = (await connection.QueryAsync<ExistingCardRow>(
                new CommandDefinition(
                    """
                    SELECT pc.[Id] AS [ComponentId], cc.[Id] AS [CardComponentId]
                    FROM [PageSections] ps
                    INNER JOIN [PageComponents] pc ON pc.[PageSectionId] = ps.[Id] AND pc.[ComponentType] = 'Card'
                    INNER JOIN [CardComponents] cc ON cc.[PageComponentId] = pc.[Id]
                    WHERE ps.[PageId] = @PageId;
                    """,
                    new { PageId = pageId },
                    tx,
                    cancellationToken: cancellationToken))).ToList();

            var existingComponentIds = existing.Select(x => x.ComponentId).ToHashSet();
            var incomingComponentIds = incoming.Where(x => x.ComponentId > 0).Select(x => x.ComponentId).ToHashSet();
            var deleteComponentIds = existingComponentIds.Except(incomingComponentIds).ToArray();

            if (deleteComponentIds.Length > 0)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "DELETE FROM [PageComponents] WHERE [Id] IN @Ids;",
                    new { Ids = deleteComponentIds },
                    tx,
                    cancellationToken: cancellationToken));
            }

            var actionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var utcNow = DateTime.UtcNow;

            foreach (var item in incoming)
            {
                int componentId;
                int cardComponentId;

                if (item.ComponentId > 0 && existingComponentIds.Contains(item.ComponentId))
                {
                    componentId = item.ComponentId;
                    cardComponentId = existing.First(x => x.ComponentId == componentId).CardComponentId;

                    await connection.ExecuteAsync(new CommandDefinition(
                        """
                        UPDATE [PageComponents]
                        SET [DisplayOrder] = @DisplayOrder, [X] = @X, [Y] = @Y, [Width] = @Width, [Height] = @Height, [IsVisible] = @IsVisible, [UpdatedDate] = @UpdatedDate
                        WHERE [Id] = @Id;
                        """,
                        new
                        {
                            Id = componentId,
                            item.DisplayOrder,
                            item.X,
                            item.Y,
                            item.Width,
                            item.Height,
                            item.IsVisible,
                            UpdatedDate = utcNow
                        },
                        tx,
                        cancellationToken: cancellationToken));

                    await connection.ExecuteAsync(new CommandDefinition(
                        """
                        UPDATE [CardComponents]
                        SET [CardDefinitionId] = @CardDefinitionId, [Title] = @Title, [Subtitle] = @Subtitle, [Description] = @Description, [HtmlFragment] = @HtmlFragment, [MediaFileId] = @MediaFileId,
                            [BackgroundColor] = @BackgroundColor, [TextColor] = @TextColor, [BorderColor] = @BorderColor, [ShowImage] = @ShowImage, [ShowButton] = @ShowButton
                        WHERE [Id] = @Id;
                        """,
                        new
                        {
                            Id = cardComponentId,
                            item.CardDefinitionId,
                            item.Title,
                            item.Subtitle,
                            item.Description,
                            item.HtmlFragment,
                            item.MediaFileId,
                            item.BackgroundColor,
                            item.TextColor,
                            item.BorderColor,
                            item.ShowImage,
                            item.ShowButton
                        },
                        tx,
                        cancellationToken: cancellationToken));
                }
                else
                {
                    componentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                        """
                        INSERT INTO [PageComponents]
                            ([PageSectionId], [Name], [ComponentType], [DisplayOrder], [X], [Y], [Width], [Height], [CssClass], [InlineStyle], [IsVisible], [CreatedDate], [UpdatedDate])
                        OUTPUT INSERTED.[Id]
                        VALUES
                            (@PageSectionId, @Name, @ComponentType, @DisplayOrder, @X, @Y, @Width, @Height, @CssClass, @InlineStyle, @IsVisible, @CreatedDate, @UpdatedDate);
                        """,
                        new
                        {
                            PageSectionId = sectionId,
                            Name = BuildCardName(item.Title),
                            ComponentType = "Card",
                            item.DisplayOrder,
                            item.X,
                            item.Y,
                            item.Width,
                            item.Height,
                            CssClass = (string?)null,
                            InlineStyle = (string?)null,
                            item.IsVisible,
                            CreatedDate = utcNow,
                            UpdatedDate = (DateTime?)null
                        },
                        tx,
                        cancellationToken: cancellationToken));

                    cardComponentId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                        """
                        INSERT INTO [CardComponents]
                            ([PageComponentId], [CardDefinitionId], [Title], [Subtitle], [Description], [HtmlFragment], [MediaFileId], [BackgroundColor], [TextColor], [BorderColor], [ShowImage], [ShowButton])
                        OUTPUT INSERTED.[Id]
                        VALUES
                            (@PageComponentId, @CardDefinitionId, @Title, @Subtitle, @Description, @HtmlFragment, @MediaFileId, @BackgroundColor, @TextColor, @BorderColor, @ShowImage, @ShowButton);
                        """,
                        new
                        {
                            PageComponentId = componentId,
                            item.CardDefinitionId,
                            item.Title,
                            item.Subtitle,
                            item.Description,
                            item.HtmlFragment,
                            item.MediaFileId,
                            item.BackgroundColor,
                            item.TextColor,
                            item.BorderColor,
                            item.ShowImage,
                            item.ShowButton
                        },
                        tx,
                        cancellationToken: cancellationToken));
                }

                var existingButtons = (await connection.QueryAsync<int>(new CommandDefinition(
                    "SELECT [Id] FROM [CardButtons] WHERE [CardComponentId] = @CardComponentId;",
                    new { CardComponentId = cardComponentId },
                    tx,
                    cancellationToken: cancellationToken))).ToHashSet();

                var incomingButtonIds = item.Buttons.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();
                var deleteButtonIds = existingButtons.Except(incomingButtonIds).ToArray();
                if (deleteButtonIds.Length > 0)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        "DELETE FROM [CardButtons] WHERE [Id] IN @Ids;",
                        new { Ids = deleteButtonIds },
                        tx,
                        cancellationToken: cancellationToken));
                }

                var buttonOrder = 1;
                foreach (var button in item.Buttons)
                {
                    var actionId = await ResolveActionIdAsync(connection, tx, button, actionCache, cancellationToken);

                    if (button.Id > 0 && existingButtons.Contains(button.Id))
                    {
                        await connection.ExecuteAsync(new CommandDefinition(
                            """
                            UPDATE [CardButtons]
                            SET [Text] = @Text, [Icon] = @Icon, [BackgroundColor] = @BackgroundColor, [TextColor] = @TextColor, [BorderColor] = @BorderColor,
                                [DisplayOrder] = @DisplayOrder, [ActionDefinitionId] = @ActionDefinitionId
                            WHERE [Id] = @Id;
                            """,
                            new
                            {
                                Id = button.Id,
                                Text = button.Text,
                                button.Icon,
                                button.BackgroundColor,
                                button.TextColor,
                                button.BorderColor,
                                DisplayOrder = buttonOrder++,
                                ActionDefinitionId = actionId
                            },
                            tx,
                            cancellationToken: cancellationToken));
                    }
                    else
                    {
                        await connection.ExecuteAsync(new CommandDefinition(
                            """
                            INSERT INTO [CardButtons] ([CardComponentId], [Text], [Icon], [BackgroundColor], [TextColor], [BorderColor], [DisplayOrder], [ActionDefinitionId])
                            VALUES (@CardComponentId, @Text, @Icon, @BackgroundColor, @TextColor, @BorderColor, @DisplayOrder, @ActionDefinitionId);
                            """,
                            new
                            {
                                CardComponentId = cardComponentId,
                                Text = button.Text,
                                button.Icon,
                                button.BackgroundColor,
                                button.TextColor,
                                button.BorderColor,
                                DisplayOrder = buttonOrder++,
                                ActionDefinitionId = actionId
                            },
                            tx,
                            cancellationToken: cancellationToken));
                    }
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task UpdateCardComponentMediaAsync(int cardComponentId, int? mediaFileId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE [CardComponents] SET [MediaFileId] = @MediaFileId WHERE [Id] = @Id;",
                new { Id = cardComponentId, MediaFileId = mediaFileId },
                cancellationToken: cancellationToken));
    }

    private static CardBuilderSaveItemInputModel Normalize(
        CardBuilderSaveItemInputModel item,
        int pageWidth,
        int pageHeight,
        int displayOrder,
        int htmlCardDefinitionId)
    {
        var width = Math.Clamp(item.Width <= 0 ? 320 : item.Width, 160, pageWidth);
        var height = Math.Clamp(item.Height <= 0 ? 220 : item.Height, 120, pageHeight);
        var xMax = Math.Max(0, pageWidth - width);
        var yMax = Math.Max(0, pageHeight - height);
        var isHtmlCard = !string.IsNullOrWhiteSpace(item.HtmlFragment);
        var buttons = isHtmlCard
            ? new List<CardBuilderSaveButtonInputModel>()
            : (item.Buttons ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .Take(10)
                .Select((x, idx) => x with
                {
                    DisplayOrder = idx + 1,
                    Text = x.Text.Trim(),
                    ActionUrl = NormalizeUrl(x.ActionUrl),
                    ActionTarget = string.IsNullOrWhiteSpace(x.ActionTarget) ? "_self" : x.ActionTarget.Trim()
                })
                .ToList();

        var htmlFragment = isHtmlCard ? item.HtmlFragment!.Trim() : null;

        return item with
        {
            DisplayOrder = displayOrder,
            X = Math.Clamp(item.X, 0, xMax),
            Y = Math.Clamp(item.Y, 0, yMax),
            Width = width,
            Height = height,
            Title = (item.Title ?? string.Empty).Trim(),
            Subtitle = string.IsNullOrWhiteSpace(item.Subtitle) ? null : item.Subtitle.Trim(),
            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
            HtmlFragment = htmlFragment,
            BackgroundColor = NormalizeColor(item.BackgroundColor, "#ffffff"),
            TextColor = NormalizeColor(item.TextColor, "#111827"),
            BorderColor = NormalizeColor(item.BorderColor, "#d1d5db"),
            CardDefinitionId = isHtmlCard
                ? htmlCardDefinitionId
                : item.CardDefinitionId is > 0
                    ? item.CardDefinitionId
                    : 1,
            ShowImage = isHtmlCard ? false : item.ShowImage,
            ShowButton = isHtmlCard ? false : item.ShowButton,
            Buttons = buttons
        };
    }

    private static string NormalizeColor(string? color, string fallback)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return fallback;
        }

        var c = color.Trim();
        return Regex.IsMatch(c, "^#[0-9a-fA-F]{6}$") ? c : fallback;
    }

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var candidate = url.Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out _))
        {
            return candidate;
        }

        if (candidate.StartsWith("/"))
        {
            return candidate;
        }

        return null;
    }

    private static string BuildCardName(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Card";
        }

        var trimmed = title.Trim();
        return trimmed.Length <= 40 ? trimmed : $"{trimmed[..40]}...";
    }

    private static async Task<int> EnsureSectionAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction tx, int pageId, CancellationToken cancellationToken)
    {
        var sectionId = await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(
                "SELECT TOP (1) [Id] FROM [PageSections] WHERE [PageId] = @PageId ORDER BY [DisplayOrder], [Id];",
                new { PageId = pageId },
                tx,
                cancellationToken: cancellationToken));

        if (sectionId.HasValue)
        {
            return sectionId.Value;
        }

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO [PageSections] ([PageId], [Name], [SectionType], [DisplayOrder], [CssClass], [InlineStyle], [IsVisible])
                OUTPUT INSERTED.[Id]
                VALUES (@PageId, @Name, @SectionType, @DisplayOrder, @CssClass, @InlineStyle, @IsVisible);
                """,
                new
                {
                    PageId = pageId,
                    Name = "Kartlar",
                    SectionType = "Content",
                    DisplayOrder = 1,
                    CssClass = (string?)null,
                    InlineStyle = (string?)null,
                    IsVisible = true
                },
                tx,
                cancellationToken: cancellationToken));
    }

    private static async Task<int?> ResolveActionIdAsync(
        System.Data.IDbConnection connection,
        System.Data.IDbTransaction tx,
        CardBuilderSaveButtonInputModel button,
        IDictionary<string, int> actionCache,
        CancellationToken cancellationToken)
    {
        if (button.ActionDefinitionId is > 0)
        {
            return button.ActionDefinitionId;
        }

        var normalizedUrl = NormalizeUrl(button.ActionUrl);
        if (string.IsNullOrWhiteSpace(normalizedUrl))
        {
            return null;
        }

        if (actionCache.TryGetValue(normalizedUrl, out var cached))
        {
            return cached;
        }

        var existingId = await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(
                "SELECT TOP (1) [Id] FROM [ActionDefinitions] WHERE [Url] = @Url;",
                new { Url = normalizedUrl },
                tx,
                cancellationToken: cancellationToken));

        if (existingId.HasValue)
        {
            actionCache[normalizedUrl] = existingId.Value;
            return existingId.Value;
        }

        var insertedId = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO [ActionDefinitions] ([Name], [ActionType], [Url], [Target], [FunctionName], [ParametersJson], [IsActive])
                OUTPUT INSERTED.[Id]
                VALUES (@Name, @ActionType, @Url, @Target, @FunctionName, @ParametersJson, @IsActive);
                """,
                new
                {
                    Name = $"Auto Link - {normalizedUrl}",
                    ActionType = "Link",
                    Url = normalizedUrl,
                    Target = string.IsNullOrWhiteSpace(button.ActionTarget) ? "_self" : button.ActionTarget.Trim(),
                    FunctionName = (string?)null,
                    ParametersJson = (string?)null,
                    IsActive = true
                },
                tx,
                cancellationToken: cancellationToken));

        actionCache[normalizedUrl] = insertedId;
        return insertedId;
    }

    private sealed class CardButtonRow
    {
        public int Id { get; set; }
        public int CardComponentId { get; set; }
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

    private sealed class ExistingCardRow
    {
        public int ComponentId { get; set; }
        public int CardComponentId { get; set; }
    }

    private sealed class PageRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

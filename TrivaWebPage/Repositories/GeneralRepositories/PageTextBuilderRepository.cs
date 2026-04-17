using Dapper;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Repositories.GeneralRepositories;

public class PageTextBuilderRepository : IPageTextBuilderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PageTextBuilderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PageEditCanvasPageData?> GetPageEditorDataAsync(int pageId, CancellationToken cancellationToken = default)
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
                               pc.[DisplayOrder],
                               pc.[X],
                               pc.[Y],
                               pc.[Width],
                               pc.[Height],
                               pc.[IsVisible],
                               tc.[Content],
                               tc.[FontFamily],
                               tc.[FontSize],
                               tc.[FontWeight],
                               tc.[TextColor],
                               tc.[TextAlign],
                               tc.[IsBold],
                               tc.[IsItalic],
                               tc.[IsUnderline]
                           FROM [PageSections] ps
                           INNER JOIN [PageComponents] pc ON pc.[PageSectionId] = ps.[Id] AND pc.[ComponentType] = 'Text'
                           INNER JOIN [TextComponents] tc ON tc.[PageComponentId] = pc.[Id]
                           WHERE ps.[PageId] = @PageId
                           ORDER BY pc.[DisplayOrder], pc.[Id];
                           """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { PageId = pageId }, cancellationToken: cancellationToken));

        var page = await multi.ReadSingleOrDefaultAsync<PageEditCanvasPageData>();
        if (page is null)
        {
            return null;
        }

        var items = (await multi.ReadAsync<TextBoxEditorItemViewModel>()).ToList();
        page.Items = items;
        return page;
    }

    public async Task SavePageTextBoxesAsync(int pageId, IReadOnlyList<TextBoxSaveItemInputModel> items, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        using var tx = connection.BeginTransaction();

        try
        {
            const string pageSql = """
                                   SELECT TOP (1) [Id], [Name], [Width], [Height]
                                   FROM [Pages]
                                   WHERE [Id] = @PageId AND [IsDeleted] = 0;
                                   """;

            var page = await connection.QuerySingleOrDefaultAsync<(int Id, string Name, int Width, int Height)>(
                new CommandDefinition(pageSql, new { PageId = pageId }, tx, cancellationToken: cancellationToken));

            if (page == default)
            {
                throw new InvalidOperationException("Kaydedilecek sayfa bulunamadı.");
            }

            const string sectionSql = """
                                      SELECT TOP (1) [Id]
                                      FROM [PageSections]
                                      WHERE [PageId] = @PageId
                                      ORDER BY [DisplayOrder], [Id];
                                      """;

            var sectionId = await connection.QuerySingleOrDefaultAsync<int?>(
                new CommandDefinition(sectionSql, new { PageId = pageId }, tx, cancellationToken: cancellationToken));

            if (sectionId is null)
            {
                const string createSectionSql = """
                                                INSERT INTO [PageSections] ([PageId], [Name], [SectionType], [DisplayOrder], [CssClass], [InlineStyle], [IsVisible])
                                                OUTPUT INSERTED.[Id]
                                                VALUES (@PageId, @Name, @SectionType, @DisplayOrder, @CssClass, @InlineStyle, @IsVisible);
                                                """;

                sectionId = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        createSectionSql,
                        new
                        {
                            PageId = pageId,
                            Name = "Metinler",
                            SectionType = "Content",
                            DisplayOrder = 1,
                            CssClass = (string?)null,
                            InlineStyle = (string?)null,
                            IsVisible = true
                        },
                        tx,
                        cancellationToken: cancellationToken));
            }

            const string existingSql = """
                                       SELECT pc.[Id]
                                       FROM [PageSections] ps
                                       INNER JOIN [PageComponents] pc ON pc.[PageSectionId] = ps.[Id] AND pc.[ComponentType] = 'Text'
                                       WHERE ps.[PageId] = @PageId;
                                       """;

            var existingIds = (await connection.QueryAsync<int>(
                new CommandDefinition(existingSql, new { PageId = pageId }, tx, cancellationToken: cancellationToken)))
                .ToHashSet();

            var incomingIds = items
                .Where(x => x.ComponentId > 0)
                .Select(x => x.ComponentId)
                .ToHashSet();

            var deleteIds = existingIds.Except(incomingIds).ToArray();
            if (deleteIds.Length > 0)
            {
                const string deleteSql = "DELETE FROM [PageComponents] WHERE [Id] IN @Ids;";
                await connection.ExecuteAsync(
                    new CommandDefinition(deleteSql, new { Ids = deleteIds }, tx, cancellationToken: cancellationToken));
            }

            var utcNow = DateTime.UtcNow;
            var displayOrder = 1;

            foreach (var item in items)
            {
                var normalized = Normalize(item, page.Width, page.Height, displayOrder++);
                if (normalized.ComponentId > 0 && existingIds.Contains(normalized.ComponentId))
                {
                    const string updateComponentSql = """
                                                      UPDATE [PageComponents]
                                                      SET
                                                          [DisplayOrder] = @DisplayOrder,
                                                          [X] = @X,
                                                          [Y] = @Y,
                                                          [Width] = @Width,
                                                          [Height] = @Height,
                                                          [IsVisible] = @IsVisible,
                                                          [UpdatedDate] = @UpdatedDate
                                                      WHERE [Id] = @Id;
                                                      """;

                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            updateComponentSql,
                            new
                            {
                                Id = normalized.ComponentId,
                                normalized.DisplayOrder,
                                normalized.X,
                                normalized.Y,
                                normalized.Width,
                                normalized.Height,
                                normalized.IsVisible,
                                UpdatedDate = utcNow
                            },
                            tx,
                            cancellationToken: cancellationToken));

                    const string updateTextSql = """
                                                 UPDATE [TextComponents]
                                                 SET
                                                     [Content] = @Content,
                                                     [FontFamily] = @FontFamily,
                                                     [FontSize] = @FontSize,
                                                     [FontWeight] = @FontWeight,
                                                     [TextColor] = @TextColor,
                                                     [TextAlign] = @TextAlign,
                                                     [IsBold] = @IsBold,
                                                     [IsItalic] = @IsItalic,
                                                     [IsUnderline] = @IsUnderline
                                                 WHERE [PageComponentId] = @PageComponentId;
                                                 """;

                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            updateTextSql,
                            new
                            {
                                PageComponentId = normalized.ComponentId,
                                normalized.Content,
                                normalized.FontFamily,
                                normalized.FontSize,
                                normalized.FontWeight,
                                normalized.TextColor,
                                normalized.TextAlign,
                                normalized.IsBold,
                                normalized.IsItalic,
                                normalized.IsUnderline
                            },
                            tx,
                            cancellationToken: cancellationToken));
                }
                else
                {
                    const string insertComponentSql = """
                                                      INSERT INTO [PageComponents]
                                                          ([PageSectionId], [Name], [ComponentType], [DisplayOrder], [X], [Y], [Width], [Height], [CssClass], [InlineStyle], [IsVisible], [CreatedDate], [UpdatedDate])
                                                      OUTPUT INSERTED.[Id]
                                                      VALUES
                                                          (@PageSectionId, @Name, @ComponentType, @DisplayOrder, @X, @Y, @Width, @Height, @CssClass, @InlineStyle, @IsVisible, @CreatedDate, @UpdatedDate);
                                                      """;

                    var componentId = await connection.ExecuteScalarAsync<int>(
                        new CommandDefinition(
                            insertComponentSql,
                            new
                            {
                                PageSectionId = sectionId!.Value,
                                Name = BuildComponentName(item.Content),
                                ComponentType = "Text",
                                normalized.DisplayOrder,
                                normalized.X,
                                normalized.Y,
                                normalized.Width,
                                normalized.Height,
                                CssClass = (string?)null,
                                InlineStyle = (string?)null,
                                normalized.IsVisible,
                                CreatedDate = utcNow,
                                UpdatedDate = (DateTime?)null
                            },
                            tx,
                            cancellationToken: cancellationToken));

                    const string insertTextSql = """
                                                 INSERT INTO [TextComponents]
                                                     ([PageComponentId], [Content], [FontFamily], [FontSize], [FontWeight], [TextColor], [TextAlign], [IsBold], [IsItalic], [IsUnderline])
                                                 VALUES
                                                     (@PageComponentId, @Content, @FontFamily, @FontSize, @FontWeight, @TextColor, @TextAlign, @IsBold, @IsItalic, @IsUnderline);
                                                 """;

                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            insertTextSql,
                            new
                            {
                                PageComponentId = componentId,
                                normalized.Content,
                                normalized.FontFamily,
                                normalized.FontSize,
                                normalized.FontWeight,
                                normalized.TextColor,
                                normalized.TextAlign,
                                normalized.IsBold,
                                normalized.IsItalic,
                                normalized.IsUnderline
                            },
                            tx,
                            cancellationToken: cancellationToken));
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

    private static TextBoxSaveItemInputModel Normalize(TextBoxSaveItemInputModel item, int pageWidth, int pageHeight, int displayOrder)
    {
        var width = Math.Clamp(item.Width, 80, pageWidth);
        var height = Math.Clamp(item.Height, 40, pageHeight);
        var xMax = Math.Max(0, pageWidth - width);
        var yMax = Math.Max(0, pageHeight - height);

        return item with
        {
            DisplayOrder = displayOrder,
            Content = (item.Content ?? string.Empty).Trim(),
            FontFamily = string.IsNullOrWhiteSpace(item.FontFamily) ? "Inter" : item.FontFamily.Trim(),
            FontSize = Math.Clamp(item.FontSize <= 0 ? 16 : item.FontSize, 8, 144),
            FontWeight = string.IsNullOrWhiteSpace(item.FontWeight) ? null : item.FontWeight.Trim(),
            TextColor = string.IsNullOrWhiteSpace(item.TextColor) ? "#111111" : item.TextColor.Trim(),
            TextAlign = string.IsNullOrWhiteSpace(item.TextAlign) ? "left" : item.TextAlign.Trim().ToLowerInvariant(),
            X = Math.Clamp(item.X, 0, xMax),
            Y = Math.Clamp(item.Y, 0, yMax),
            Width = width,
            Height = height
        };
    }

    private static string BuildComponentName(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Text Box";
        }

        var trimmed = content.Trim();
        return trimmed.Length <= 40 ? trimmed : $"{trimmed[..40]}...";
    }
}

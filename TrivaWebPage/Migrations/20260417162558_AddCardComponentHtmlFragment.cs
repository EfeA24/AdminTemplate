using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class AddCardComponentHtmlFragment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlFragment",
                table: "CardComponents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'html-embed')
                BEGIN
                    INSERT INTO [CardDefinitions] ([Name], [Code], [CardType], [Description], [PreviewMediaFileId], [IsActive])
                    VALUES (N'HTML gömülü', N'html-embed', N'Custom', N'Ham HTML ile kart içeriği.', NULL, 1);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE cd
                FROM [CardDefinitions] cd
                WHERE cd.[Code] = N'html-embed'
                  AND NOT EXISTS (SELECT 1 FROM [CardComponents] cc WHERE cc.[CardDefinitionId] = cd.[Id]);
                """);

            migrationBuilder.DropColumn(
                name: "HtmlFragment",
                table: "CardComponents");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class SeedMenuStyleCardDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'menu-nav-h')
                INSERT INTO [CardDefinitions] ([Name],[Code],[PreviewImageUrl],[Description],[IsSystem],[CreatedDate],[UpdatedDate])
                VALUES (N'Menü (yatay)', N'menu-nav-h', N'/pictures/placeholder-card.svg', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'menu-nav-v')
                INSERT INTO [CardDefinitions] ([Name],[Code],[PreviewImageUrl],[Description],[IsSystem],[CreatedDate],[UpdatedDate])
                VALUES (N'Menü (dikey)', N'menu-nav-v', N'/pictures/placeholder-card.svg', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'menu-pills')
                INSERT INTO [CardDefinitions] ([Name],[Code],[PreviewImageUrl],[Description],[IsSystem],[CreatedDate],[UpdatedDate])
                VALUES (N'Menü (pill)', N'menu-pills', N'/pictures/placeholder-card.svg', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'menu-icon-row')
                INSERT INTO [CardDefinitions] ([Name],[Code],[PreviewImageUrl],[Description],[IsSystem],[CreatedDate],[UpdatedDate])
                VALUES (N'Menü (ikon şeridi)', N'menu-icon-row', N'/pictures/placeholder-card.svg', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [CardDefinitions] WHERE [Code] IN (N'menu-nav-h', N'menu-nav-v', N'menu-pills', N'menu-icon-row');
                """);
        }
    }
}

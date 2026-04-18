using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class RebuildCardDefinitionsAsTemplates : Migration
    {
        private const string PlaceholderPreview = "/pictures/placeholder-card.svg";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardDefinitions_MediaFiles_PreviewMediaFileId",
                table: "CardDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_CardDefinitions_PreviewMediaFileId",
                table: "CardDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "CardDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "CardDefinitions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "CardDefinitions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            var ph = PlaceholderPreview.Replace("'", "''");
            migrationBuilder.Sql($"""
                UPDATE [CardDefinitions]
                SET
                    [CreatedDate] = SYSUTCDATETIME(),
                    [UpdatedDate] = SYSUTCDATETIME(),
                    [PreviewImageUrl] = COALESCE(NULLIF(LTRIM(RTRIM([PreviewImageUrl])), N''), N'{ph}'),
                    [IsSystem] = CASE WHEN [Code] IN (N'info', N'cta', N'image', N'html-embed') THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END;

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'info')
                INSERT INTO [CardDefinitions] ([Name], [Code], [CardType], [Description], [PreviewMediaFileId], [IsActive], [PreviewHtml], [PreviewImageUrl], [IsSystem], [CreatedDate], [UpdatedDate])
                VALUES (N'Bilgi Kartı', N'info', N'Info', NULL, NULL, 1, NULL, N'{ph}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'cta')
                INSERT INTO [CardDefinitions] ([Name], [Code], [CardType], [Description], [PreviewMediaFileId], [IsActive], [PreviewHtml], [PreviewImageUrl], [IsSystem], [CreatedDate], [UpdatedDate])
                VALUES (N'Aksiyon Kartı', N'cta', N'Cta', NULL, NULL, 1, NULL, N'{ph}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [CardDefinitions] WHERE [Code] = N'image')
                INSERT INTO [CardDefinitions] ([Name], [Code], [CardType], [Description], [PreviewMediaFileId], [IsActive], [PreviewHtml], [PreviewImageUrl], [IsSystem], [CreatedDate], [UpdatedDate])
                VALUES (N'Görselli Kart', N'image', N'Image', NULL, NULL, 1, NULL, N'{ph}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                UPDATE [CardDefinitions] SET [Name] = N'Bilgi Kartı', [IsSystem] = 1, [PreviewImageUrl] = COALESCE(NULLIF(LTRIM(RTRIM([PreviewImageUrl])), N''), N'{ph}'), [UpdatedDate] = SYSUTCDATETIME() WHERE [Code] = N'info';
                UPDATE [CardDefinitions] SET [Name] = N'Aksiyon Kartı', [IsSystem] = 1, [PreviewImageUrl] = COALESCE(NULLIF(LTRIM(RTRIM([PreviewImageUrl])), N''), N'{ph}'), [UpdatedDate] = SYSUTCDATETIME() WHERE [Code] = N'cta';
                UPDATE [CardDefinitions] SET [Name] = N'Görselli Kart', [IsSystem] = 1, [PreviewImageUrl] = COALESCE(NULLIF(LTRIM(RTRIM([PreviewImageUrl])), N''), N'{ph}'), [UpdatedDate] = SYSUTCDATETIME() WHERE [Code] = N'image';

                DECLARE @infoId INT = (SELECT TOP (1) [Id] FROM [CardDefinitions] WHERE [Code] = N'info');
                IF @infoId IS NOT NULL
                BEGIN
                    UPDATE cc SET cc.[CardDefinitionId] = @infoId
                    FROM [CardComponents] AS cc
                    INNER JOIN [CardDefinitions] AS cd ON cd.[Id] = cc.[CardDefinitionId]
                    WHERE cd.[Code] = N'html-embed';
                END

                DELETE FROM [CardDefinitions] WHERE [Code] = N'html-embed';
                """);

            migrationBuilder.DropColumn(
                name: "PreviewHtml",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "PreviewMediaFileId",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "CardType",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CardDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImageUrl",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: PlaceholderPreview,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CardDefinitions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CardDefinitions_Code",
                table: "CardDefinitions",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CardDefinitions_Code",
                table: "CardDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImageUrl",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Info");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CardDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewHtml",
                table: "CardDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviewMediaFileId",
                table: "CardDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "CardDefinitions");

            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "CardDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_CardDefinitions_PreviewMediaFileId",
                table: "CardDefinitions",
                column: "PreviewMediaFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardDefinitions_MediaFiles_PreviewMediaFileId",
                table: "CardDefinitions",
                column: "PreviewMediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

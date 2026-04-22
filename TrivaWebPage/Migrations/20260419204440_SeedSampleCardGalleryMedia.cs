using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleCardGalleryMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [MediaFiles] WHERE [FilePath] = N'/pictures/template-previews/dark-main.png')
                INSERT INTO [MediaFiles] ([FileName],[OriginalFileName],[FilePath],[AltText],[ContentType],[FileSize],[FileExtension],[Width],[Height],[UploadedDate])
                VALUES (N'dark-main.png', N'dark-main.png', N'/pictures/template-previews/dark-main.png', NULL, N'image/png', 0, N'.png', NULL, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [MediaFiles] WHERE [FilePath] = N'/pictures/template-previews/dark-gallery.png')
                INSERT INTO [MediaFiles] ([FileName],[OriginalFileName],[FilePath],[AltText],[ContentType],[FileSize],[FileExtension],[Width],[Height],[UploadedDate])
                VALUES (N'dark-gallery.png', N'dark-gallery.png', N'/pictures/template-previews/dark-gallery.png', NULL, N'image/png', 0, N'.png', NULL, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [MediaFiles] WHERE [FilePath] = N'/pictures/template-previews/creative-main.png')
                INSERT INTO [MediaFiles] ([FileName],[OriginalFileName],[FilePath],[AltText],[ContentType],[FileSize],[FileExtension],[Width],[Height],[UploadedDate])
                VALUES (N'creative-main.png', N'creative-main.png', N'/pictures/template-previews/creative-main.png', NULL, N'image/png', 0, N'.png', NULL, NULL, SYSUTCDATETIME());

                IF NOT EXISTS (SELECT 1 FROM [MediaFiles] WHERE [FilePath] = N'/pictures/template-previews/triva-ana-sayfa-1-preview.png')
                INSERT INTO [MediaFiles] ([FileName],[OriginalFileName],[FilePath],[AltText],[ContentType],[FileSize],[FileExtension],[Width],[Height],[UploadedDate])
                VALUES (N'triva-ana-sayfa-1-preview.png', N'triva-ana-sayfa-1-preview.png', N'/pictures/template-previews/triva-ana-sayfa-1-preview.png', NULL, N'image/png', 0, N'.png', NULL, NULL, SYSUTCDATETIME());

                DECLARE @pageId INT = (SELECT TOP (1) [Id] FROM [Pages] WHERE [IsDeleted] = 0 ORDER BY [DisplayOrder], [Id]);
                IF @pageId IS NOT NULL
                BEGIN
                    INSERT INTO [PageMediaFiles] ([PageId], [MediaFileId])
                    SELECT @pageId, m.[Id]
                    FROM [MediaFiles] m
                    WHERE m.[FilePath] IN (
                        N'/pictures/template-previews/dark-main.png',
                        N'/pictures/template-previews/dark-gallery.png',
                        N'/pictures/template-previews/creative-main.png',
                        N'/pictures/template-previews/triva-ana-sayfa-1-preview.png'
                    )
                    AND NOT EXISTS (
                        SELECT 1 FROM [PageMediaFiles] pmf
                        WHERE pmf.[PageId] = @pageId AND pmf.[MediaFileId] = m.[Id]
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE pmf FROM [PageMediaFiles] pmf
                INNER JOIN [MediaFiles] m ON m.[Id] = pmf.[MediaFileId]
                WHERE m.[FilePath] IN (
                    N'/pictures/template-previews/dark-main.png',
                    N'/pictures/template-previews/dark-gallery.png',
                    N'/pictures/template-previews/creative-main.png',
                    N'/pictures/template-previews/triva-ana-sayfa-1-preview.png'
                );

                DELETE m FROM [MediaFiles] m
                WHERE m.[FilePath] IN (
                    N'/pictures/template-previews/dark-main.png',
                    N'/pictures/template-previews/dark-gallery.png',
                    N'/pictures/template-previews/creative-main.png',
                    N'/pictures/template-previews/triva-ana-sayfa-1-preview.png'
                )
                AND NOT EXISTS (SELECT 1 FROM [CardComponents] c WHERE c.[MediaFileId] = m.[Id])
                AND NOT EXISTS (SELECT 1 FROM [ImageComponents] i WHERE i.[MediaFileId] = m.[Id]);
                """);
        }
    }
}

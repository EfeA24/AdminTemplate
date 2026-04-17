using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatePagePreviewImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewImagePath",
                table: "PageTemplatePages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImagePath",
                table: "PageTemplatePages");
        }
    }
}

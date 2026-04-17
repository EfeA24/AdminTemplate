using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaWebPage.Migrations
{
    /// <inheritdoc />
    public partial class AddPageTemplatesAndColorPalettes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorPaletteId",
                table: "Pages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageTemplatePageId",
                table: "Pages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ColorPalettes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MutedHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccentHex = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorPalettes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageTemplatePages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageTemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTemplatePages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageTemplatePages_PageTemplates_PageTemplateId",
                        column: x => x.PageTemplateId,
                        principalTable: "PageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ColorPaletteId",
                table: "Pages",
                column: "ColorPaletteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_PageTemplatePageId",
                table: "Pages",
                column: "PageTemplatePageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTemplatePages_PageTemplateId",
                table: "PageTemplatePages",
                column: "PageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PageTemplates_Code",
                table: "PageTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_ColorPalettes_ColorPaletteId",
                table: "Pages",
                column: "ColorPaletteId",
                principalTable: "ColorPalettes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_PageTemplatePages_PageTemplatePageId",
                table: "Pages",
                column: "PageTemplatePageId",
                principalTable: "PageTemplatePages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                INSERT INTO [ColorPalettes] ([Name], [PrimaryHex], [SecondaryHex], [MutedHex], [AccentHex]) VALUES
                (N'Limon – Orman yeşili', N'#ccda47', N'#0a3625', N'#8a9a6b', N'#145a40'),
                (N'Amber – Petrol', N'#f2b759', N'#0a4a3c', N'#b89a76', N'#1a6b58'),
                (N'Krem – Bordo', N'#f2efe7', N'#8b004a', N'#d4c4cc', N'#a3005c'),
                (N'Kum – Turkuaz', N'#e4ddd3', N'#00a19b', N'#b4cec8', N'#33c4bd'),
                (N'Turuncu – Antrasit', N'#c87740', N'#2e1f26', N'#9a6b58', N'#5a4038'),
                (N'Kömür – Neon lime', N'#222222', N'#89e900', N'#5a5a5a', N'#a5ff33'),
                (N'Buz – İndigo', N'#f7f7ff', N'#27187e', N'#c5c5e6', N'#4a3fa8'),
                (N'Petrol – Krem', N'#004643', N'#f0ede5', N'#6d9995', N'#2a9993'),
                (N'Mürdüm – Fildişi', N'#381932', N'#fff3e6', N'#7d6578', N'#5c2d52');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_ColorPalettes_ColorPaletteId",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_Pages_PageTemplatePages_PageTemplatePageId",
                table: "Pages");

            migrationBuilder.DropTable(
                name: "ColorPalettes");

            migrationBuilder.DropTable(
                name: "PageTemplatePages");

            migrationBuilder.DropTable(
                name: "PageTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Pages_ColorPaletteId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_PageTemplatePageId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "ColorPaletteId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageTemplatePageId",
                table: "Pages");
        }
    }
}

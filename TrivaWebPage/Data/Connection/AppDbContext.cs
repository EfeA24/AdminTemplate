using Microsoft.EntityFrameworkCore;
using TrivaWebPage.Models;
using TrivaWebPage.Models.CardOptions;
using TrivaWebPage.Models.Contents;
using TrivaWebPage.Models.General;

namespace TrivaWebPage.Data.Connection
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Page> Pages => Set<Page>();
        public DbSet<PageTemplate> PageTemplates => Set<PageTemplate>();
        public DbSet<PageTemplatePage> PageTemplatePages => Set<PageTemplatePage>();
        public DbSet<ColorPalette> ColorPalettes => Set<ColorPalette>();
        public DbSet<PageSection> PageSections => Set<PageSection>();
        public DbSet<PageComponent> PageComponents => Set<PageComponent>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
        public DbSet<PageMediaFile> PageMediaFiles => Set<PageMediaFile>();

        public DbSet<TextComponent> TextComponents => Set<TextComponent>();
        public DbSet<ImageComponent> ImageComponents => Set<ImageComponent>();
        public DbSet<ButtonComponent> ButtonComponents => Set<ButtonComponent>();
        public DbSet<CardComponent> CardComponents => Set<CardComponent>();

        public DbSet<ActionDefinition> ActionDefinitions => Set<ActionDefinition>();
        public DbSet<CardDefinition> CardDefinitions => Set<CardDefinition>();
        public DbSet<CardFieldDefinition> CardFieldDefinitions => Set<CardFieldDefinition>();
        public DbSet<CardFieldValue> CardFieldValues => Set<CardFieldValue>();
        public DbSet<CardButton> CardButtons => Set<CardButton>();

        public DbSet<WebsiteSettings> WebsiteSettings => Set<WebsiteSettings>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PageTemplate>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasMany(t => t.Pages)
                    .WithOne(p => p.PageTemplate)
                    .HasForeignKey(p => p.PageTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ColorPalette>(entity =>
            {
                entity.ToTable("ColorPalettes");
            });

            modelBuilder.Entity<Page>(entity =>
            {
                entity.HasOne(p => p.PageTemplatePage)
                    .WithMany(t => t.Pages)
                    .HasForeignKey(p => p.PageTemplatePageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.ColorPalette)
                    .WithMany(c => c.Pages)
                    .HasForeignKey(p => p.ColorPaletteId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.Sections)
                    .WithOne(s => s.Page)
                    .HasForeignKey(s => s.PageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.PageMediaFiles)
                    .WithOne(l => l.Page)
                    .HasForeignKey(l => l.PageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageMediaFile>(entity =>
            {
                entity.ToTable("PageMediaFiles");
                entity.HasIndex(e => new { e.PageId, e.MediaFileId }).IsUnique();

                entity.HasOne(e => e.MediaFile)
                    .WithMany(m => m.PageMediaFiles)
                    .HasForeignKey(e => e.MediaFileId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageSection>(entity =>
            {
                entity.HasMany(s => s.Components)
                    .WithOne(c => c.PageSection)
                    .HasForeignKey(c => c.PageSectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PageComponent>(entity =>
            {
                entity.HasOne(p => p.TextComponent)
                    .WithOne(t => t.PageComponent)
                    .HasForeignKey<TextComponent>(t => t.PageComponentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ImageComponent)
                    .WithOne(i => i.PageComponent)
                    .HasForeignKey<ImageComponent>(i => i.PageComponentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ButtonComponent)
                    .WithOne(b => b.PageComponent)
                    .HasForeignKey<ButtonComponent>(b => b.PageComponentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.CardComponent)
                    .WithOne(c => c.PageComponent)
                    .HasForeignKey<CardComponent>(c => c.PageComponentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ImageComponent>(entity =>
            {
                entity.HasOne(i => i.MediaFile)
                    .WithMany(m => m.ImageComponents)
                    .HasForeignKey(i => i.MediaFileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ButtonComponent>(entity =>
            {
                entity.HasOne(b => b.ActionDefinition)
                    .WithMany(a => a.ButtonComponents)
                    .HasForeignKey(b => b.ActionDefinitionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CardComponent>(entity =>
            {
                entity.HasOne(c => c.CardDefinition)
                    .WithMany(d => d.CardComponents)
                    .HasForeignKey(c => c.CardDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.MediaFile)
                    .WithMany(m => m.CardComponents)
                    .HasForeignKey(c => c.MediaFileId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(c => c.FieldValues)
                    .WithOne(v => v.CardComponent)
                    .HasForeignKey(v => v.CardComponentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Buttons)
                    .WithOne(b => b.CardComponent)
                    .HasForeignKey(b => b.CardComponentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CardDefinition>(entity =>
            {
                entity.HasOne(d => d.PreviewMediaFile)
                    .WithMany(m => m.CardDefinitions)
                    .HasForeignKey(d => d.PreviewMediaFileId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(d => d.FieldDefinitions)
                    .WithOne(f => f.CardDefinition)
                    .HasForeignKey(f => f.CardDefinitionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CardFieldValue>(entity =>
            {
                entity.HasOne(v => v.CardFieldDefinition)
                    .WithMany()
                    .HasForeignKey(v => v.CardFieldDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CardButton>(entity =>
            {
                entity.HasOne(b => b.ActionDefinition)
                    .WithMany(a => a.CardButtons)
                    .HasForeignKey(b => b.ActionDefinitionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(e => e.UserName).IsUnique();
            });
        }
    }
}


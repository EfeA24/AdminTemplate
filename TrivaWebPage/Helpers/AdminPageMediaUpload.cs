using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TrivaWebPage.Abstractions.GeneralAbstactions;
using TrivaWebPage.Models.General;
using TrivaWebPage.ViewModels.Admin;

namespace TrivaWebPage.Helpers;

public static class AdminPageMediaUpload
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    public sealed record UploadOutcome(bool Success, string? ErrorMessage, int MediaFileId, string FilePath, string DisplayName);

    public static async Task<IReadOnlyList<PageEditMediaItemViewModel>> LoadPageMediaAsync(
        int pageId,
        IPageMediaFile pageMediaFile,
        IMediaFile mediaFile,
        CancellationToken cancellationToken)
    {
        var mediaIds = await pageMediaFile.GetMediaFileIdsByPageAsync(pageId, cancellationToken);
        var mediaList = new List<PageEditMediaItemViewModel>();
        foreach (var mid in mediaIds)
        {
            var m = await mediaFile.GetByIdAsync(mid, cancellationToken);
            if (m is null) continue;
            mediaList.Add(new PageEditMediaItemViewModel
            {
                MediaFileId = m.Id,
                FilePath = m.FilePath,
                DisplayName = m.OriginalFileName
            });
        }

        return mediaList;
    }

    /// <summary>
    /// Creates a <see cref="MediaFile"/> row and stored file under wwwroot (same rules as page upload), without linking to a page.
    /// </summary>
    public static async Task<UploadOutcome> TryCreateMediaFileAsync(
        IFormFile? file,
        IWebHostEnvironment environment,
        IMediaFile mediaFile,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return new UploadOutcome(false, "Lütfen bir dosya seçin.", 0, "", "");
        }

        if (file.Length > MaxUploadBytes)
        {
            return new UploadOutcome(false, "Dosya boyutu en fazla 10 MB olabilir.", 0, "", "");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return new UploadOutcome(false, "Yalnızca JPG, PNG, GIF veya WEBP yükleyebilirsiniz.", 0, "", "");
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            return new UploadOutcome(false, "Geçersiz dosya türü.", 0, "", "");
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var relativeDir = Path.Combine("uploads", "media");
        var physicalDir = Path.Combine(webRoot, relativeDir);
        Directory.CreateDirectory(physicalDir);

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(physicalDir, storedName);

        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var webPath = "/" + relativeDir.Replace(Path.DirectorySeparatorChar, '/') + "/" + storedName;
        var displayName = Path.GetFileName(file.FileName);

        var entity = new MediaFile
        {
            FileName = storedName,
            OriginalFileName = displayName,
            FilePath = webPath,
            AltText = null,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileExtension = ext.TrimStart('.'),
            Width = null,
            Height = null,
            UploadedDate = DateTime.UtcNow
        };

        var mediaId = await mediaFile.CreateAsync(entity, cancellationToken);
        return new UploadOutcome(true, null, mediaId, webPath, displayName);
    }

    public static async Task<UploadOutcome> TryUploadAndLinkAsync(
        IFormFile? file,
        IWebHostEnvironment environment,
        IMediaFile mediaFile,
        int pageId,
        IPageMediaFile pageMediaFile,
        CancellationToken cancellationToken)
    {
        var created = await TryCreateMediaFileAsync(file, environment, mediaFile, cancellationToken);
        if (!created.Success)
        {
            return created;
        }

        await pageMediaFile.EnsureLinkedAsync(pageId, created.MediaFileId, cancellationToken);
        return created;
    }
}

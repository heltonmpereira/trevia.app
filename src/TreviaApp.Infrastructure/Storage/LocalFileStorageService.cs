namespace TreviaApp.Infrastructure.Storage;

using Microsoft.Extensions.Options;
using TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Contracts.Common;

public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalFileStorageOptions _options;
    private readonly HashSet<string> _allowedContentTypes = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/gif", "image/webp", "video/mp4", "image/jpg" };

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options) => _options = options.Value;

    public Task<bool> ValidateFileAsync(long sizeBytes, string contentType, CancellationToken cancellationToken = default)
    {
        if (sizeBytes <= 0) return Task.FromResult(false);
        if (sizeBytes > _options.MaxFileSizeBytes) return Task.FromResult(false);
        return Task.FromResult(_allowedContentTypes.Contains(contentType));
    }

    public async Task<FileResult> UploadAsync(string ownerId, string category, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!await ValidateFileAsync(fileStream.Length, contentType, cancellationToken))
            throw new InvalidOperationException("Arquivo inválido (tamanho ou tipo não permitido).");

        var baseDir = string.IsNullOrWhiteSpace(_options.RootPath) ? Path.Combine(Path.GetTempPath(), "treviaapp-storage") : _options.RootPath;
        var categoryDir = Path.Combine(baseDir, ownerId, category);
        Directory.CreateDirectory(categoryDir);

        var fileId = Guid.NewGuid().ToString("N");
        var ext = Path.GetExtension(fileName);
        var storedName = fileId + ext;
        var fullPath = Path.Combine(categoryDir, storedName);

        await using (var fs = File.Create(fullPath))
            await fileStream.CopyToAsync(fs, cancellationToken);

        var size = new FileInfo(fullPath).Length;
        return new FileResult(fileName, contentType, size, fullPath, null);
    }

    public Task<FileResult?> GetAsync(string fileId, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(fileId)) return Task.FromResult<FileResult?>(null);
        var fi = new FileInfo(fileId);
        var res = new FileResult(Path.GetFileName(fileId), "", fi.Length, fileId, null);
        return Task.FromResult<FileResult?>(res);
    }

    public Task<string> GetTemporaryUrlAsync(string fileId, TimeSpan validFor, CancellationToken cancellationToken = default) => Task.FromResult(fileId);

    public Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        if (File.Exists(fileId)) File.Delete(fileId);
        return Task.CompletedTask;
    }
}

public class LocalFileStorageOptions
{
    public string RootPath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 50L * 1024 * 1024;
}

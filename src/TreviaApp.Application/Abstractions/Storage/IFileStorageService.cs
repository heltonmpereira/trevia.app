namespace TreviaApp.Application.Abstractions.Storage;
using TreviaApp.Contracts.Common;

public interface IFileStorageService
{
    Task<FileResult> UploadAsync(string ownerId, string category, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileId, CancellationToken cancellationToken = default);
    Task<FileResult?> GetAsync(string fileId, CancellationToken cancellationToken = default);
    Task<string> GetTemporaryUrlAsync(string fileId, TimeSpan validFor, CancellationToken cancellationToken = default);
    Task<bool> ValidateFileAsync(long sizeBytes, string contentType, CancellationToken cancellationToken = default);
}

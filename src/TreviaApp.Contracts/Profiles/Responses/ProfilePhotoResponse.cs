namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for ProfilePhotoResponse.
/// </summary>
/// <param name="Id">Id value.</param>
/// <param name="FileName">File Name value.</param>
/// <param name="ContentType">Content Type value.</param>
/// <param name="SizeBytes">Size Bytes value.</param>
/// <param name="UploadedAt">Uploaded At value.</param>
/// <param name="AccessUrl">Access Url value.</param>
public sealed record ProfilePhotoResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAt,
    string? AccessUrl);

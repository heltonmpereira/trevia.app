namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record ProfilePhotoResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAt,
    string? AccessUrl);

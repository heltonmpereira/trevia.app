namespace TreviaApp.Contracts.Profiles.Responses;

public sealed record PhotoUploadResultResponse(
    bool Success,
    ProfilePhotoResponse? Photo,
    string? ErrorMessage = null);

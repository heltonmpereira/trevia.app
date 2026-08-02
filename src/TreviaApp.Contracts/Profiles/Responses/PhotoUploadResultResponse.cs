namespace TreviaApp.Contracts.Profiles.Responses;

/// <summary>
/// Response payload for PhotoUploadResultResponse.
/// </summary>
/// <param name="Success">Success value.</param>
/// <param name="Photo">Photo value.</param>
/// <param name="ErrorMessage">Error Message value.</param>
public sealed record PhotoUploadResultResponse(
    bool Success,
    ProfilePhotoResponse? Photo,
    string? ErrorMessage = null);

using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

/// <summary>
/// Represents the ProfilePhoto domain entity.
/// </summary>
public class ProfilePhoto : Entity
{
    /// <summary>
    /// Gets Profile Id.
    /// </summary>
    public Guid ProfileId { get; private set; }
    /// <summary>
    /// Gets Profile.
    /// </summary>
    public UserProfile Profile { get; private set; } = null!;
    /// <summary>
    /// Gets File Id.
    /// </summary>
    public string FileId { get; private set; }
    /// <summary>
    /// Gets File Name.
    /// </summary>
    public string FileName { get; private set; }
    /// <summary>
    /// Gets Content Type.
    /// </summary>
    public string ContentType { get; private set; }
    /// <summary>
    /// Gets Size Bytes.
    /// </summary>
    public long SizeBytes { get; private set; }
    /// <summary>
    /// Gets Uploaded At.
    /// </summary>
    public DateTimeOffset UploadedAt { get; private set; }

    private ProfilePhoto() { }

    /// <summary>
    /// Initializes a new instance of the ProfilePhoto class.
    /// </summary>
    public ProfilePhoto(Guid profileId, string fileId, string fileName, string contentType, long sizeBytes)
    {
        if (profileId == Guid.Empty) throw new ArgumentException(nameof(profileId));
        if (string.IsNullOrWhiteSpace(fileId)) throw new ArgumentException(nameof(fileId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException(nameof(contentType));
        if (sizeBytes < 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));

        ProfileId = profileId;
        FileId = fileId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

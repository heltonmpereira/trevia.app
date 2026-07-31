using TreviaApp.Domain.Abstractions;

namespace TreviaApp.Domain.Profiles;

public class ProfilePhoto : Entity
{
    public Guid ProfileId { get; private set; }
    public UserProfile Profile { get; private set; } = null!;
    public string FileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private ProfilePhoto() { }

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

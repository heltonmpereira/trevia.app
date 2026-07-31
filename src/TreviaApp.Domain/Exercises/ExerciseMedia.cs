using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

public class ExerciseMedia : Entity
{
    public Guid ExerciseId { get; private set; }
    public Exercise Exercise { get; private set; } = null!;

    public string FileId { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public MediaType MediaType { get; private set; }
    public int Order { get; private set; }
    public string? Caption { get; private set; }
    public bool IsPrimary { get; private set; }
    public long SizeBytes { get; private set; }

    private ExerciseMedia() { }

    public ExerciseMedia(Guid exerciseId, string fileId, string fileName, MediaType mediaType,
                         int order, string? caption = null, bool isPrimary = false, long sizeBytes = 0)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException(nameof(exerciseId));
        if (string.IsNullOrWhiteSpace(fileId)) throw new ArgumentException(nameof(fileId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(nameof(fileName));

        ExerciseId = exerciseId;
        FileId = fileId;
        FileName = fileName;
        MediaType = mediaType;
        Order = order;
        Caption = caption;
        IsPrimary = isPrimary;
        SizeBytes = sizeBytes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPrimary(bool isPrimary) { IsPrimary = isPrimary; UpdatedAt = DateTimeOffset.UtcNow; }
    public void SetOrder(int order) { Order = order; UpdatedAt = DateTimeOffset.UtcNow; }
}

using TreviaApp.Domain.Abstractions;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

/// <summary>
/// Represents the ExerciseMedia domain entity.
/// </summary>
public class ExerciseMedia : Entity
{
    /// <summary>
    /// Gets Exercise Id.
    /// </summary>
    public Guid ExerciseId { get; private set; }
    /// <summary>
    /// Gets Exercise.
    /// </summary>
    public Exercise Exercise { get; private set; } = null!;

    /// <summary>
    /// Gets File Id.
    /// </summary>
    public string FileId { get; private set; } = null!;
    /// <summary>
    /// Gets File Name.
    /// </summary>
    public string FileName { get; private set; } = null!;
    /// <summary>
    /// Gets Media Type.
    /// </summary>
    public MediaType MediaType { get; private set; }
    /// <summary>
    /// Gets Order.
    /// </summary>
    public int Order { get; private set; }
    /// <summary>
    /// Gets Caption.
    /// </summary>
    public string? Caption { get; private set; }
    /// <summary>
    /// Gets Is Primary.
    /// </summary>
    public bool IsPrimary { get; private set; }
    /// <summary>
    /// Gets Size Bytes.
    /// </summary>
    public long SizeBytes { get; private set; }

    private ExerciseMedia() { }

    /// <summary>
    /// Initializes a new instance of the ExerciseMedia class.
    /// </summary>
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

    /// <summary>
    /// Executes Set Primary.
    /// </summary>
    public void SetPrimary(bool isPrimary) { IsPrimary = isPrimary; UpdatedAt = DateTimeOffset.UtcNow; }
    /// <summary>
    /// Executes Set Order.
    /// </summary>
    public void SetOrder(int order) { Order = order; UpdatedAt = DateTimeOffset.UtcNow; }
}

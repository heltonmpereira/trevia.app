using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.Exercises;

/// <summary>
/// Represents the Exercise domain entity.
/// </summary>
public class Exercise : AggregateRoot
{
    /// <summary>
    /// Gets Name.
    /// </summary>
    public string Name { get; private set; } = null!;
    /// <summary>
    /// Gets Slug.
    /// </summary>
    public string Slug { get; private set; } = null!;
    /// <summary>
    /// Gets Short Description.
    /// </summary>
    public string? ShortDescription { get; private set; }
    /// <summary>
    /// Gets Instructions.
    /// </summary>
    public string Instructions { get; private set; } = null!;
    /// <summary>
    /// Gets Tips.
    /// </summary>
    public string? Tips { get; private set; }
    /// <summary>
    /// Gets Tags.
    /// </summary>
    public string? Tags { get; private set; }

    /// <summary>
    /// Gets Environment.
    /// </summary>
    public TrainingEnvironment Environment { get; private set; }
    /// <summary>
    /// Gets Modality.
    /// </summary>
    public ExerciseModality Modality { get; private set; }
    /// <summary>
    /// Gets Difficulty Level.
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; private set; }
    /// <summary>
    /// Gets Measurement Type.
    /// </summary>
    public MeasurementType MeasurementType { get; private set; }
    /// <summary>
    /// Gets Visibility.
    /// </summary>
    public Visibility Visibility { get; private set; } = Visibility.Private;
    /// <summary>
    /// Gets Status.
    /// </summary>
    public ExerciseStatus Status { get; private set; } = ExerciseStatus.Draft;

    /// <summary>
    /// Gets Created By User Id.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }
    /// <summary>
    /// Gets Created By User.
    /// </summary>
    public AppUser CreatedByUser { get; private set; } = null!;
    /// <summary>
    /// Gets Approved By User Id.
    /// </summary>
    public Guid? ApprovedByUserId { get; private set; }
    /// <summary>
    /// Gets Approved By User.
    /// </summary>
    public AppUser? ApprovedByUser { get; private set; }
    /// <summary>
    /// Gets Approved At.
    /// </summary>
    public DateTimeOffset? ApprovedAt { get; private set; }
    /// <summary>
    /// Gets Reject Reason.
    /// </summary>
    public string? RejectReason { get; private set; }
    /// <summary>
    /// Gets Rejected At.
    /// </summary>
    public DateTimeOffset? RejectedAt { get; private set; }
    /// <summary>
    /// Gets Rejected By User Id.
    /// </summary>
    public Guid? RejectedByUserId { get; private set; }
    /// <summary>
    /// Gets Rejected By User.
    /// </summary>
    public AppUser? RejectedByUser { get; private set; }

    private readonly List<ExerciseMuscle> _muscles = new();
    /// <summary>
    /// Gets Muscles.
    /// </summary>
    public IReadOnlyCollection<ExerciseMuscle> Muscles => _muscles.AsReadOnly();

    private readonly List<ExerciseEquipment> _equipments = new();
    /// <summary>
    /// Gets Equipments.
    /// </summary>
    public IReadOnlyCollection<ExerciseEquipment> Equipments => _equipments.AsReadOnly();

    private readonly List<ExerciseMedia> _medias = new();
    /// <summary>
    /// Gets Medias.
    /// </summary>
    public IReadOnlyCollection<ExerciseMedia> Medias => _medias.AsReadOnly();

    private Exercise() { }

    /// <summary>
    /// Initializes a new instance of the Exercise class.
    /// </summary>
    public Exercise(
        Guid createdByUserId,
        string name,
        string slug,
        TrainingEnvironment environment,
        ExerciseModality modality,
        DifficultyLevel difficultyLevel,
        MeasurementType measurementType,
        string instructions,
        string? shortDescription = null,
        string? tips = null,
        string? tags = null,
        Visibility visibility = Visibility.Private)
    {
        ValidateCtorParams(createdByUserId, name, slug, instructions);

        CreatedByUserId = createdByUserId;
        Name = name;
        Slug = slug;
        Environment = environment;
        Modality = modality;
        DifficultyLevel = difficultyLevel;
        MeasurementType = measurementType;
        Instructions = instructions;
        ShortDescription = shortDescription;
        Tips = tips;
        Tags = tags;
        Visibility = visibility;
        Status = ExerciseStatus.Draft;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateCtorParams(Guid createdByUserId, string name, string slug, string instructions)
    {
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty.", nameof(createdByUserId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 200) throw new ArgumentException("Name too long (> 200).", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        if (slug.Length > 250) throw new ArgumentException("Slug too long.", nameof(slug));
        if (string.IsNullOrWhiteSpace(instructions)) throw new ArgumentException("Instructions cannot be empty.", nameof(instructions));
        if (instructions.Length > 4000) throw new ArgumentException("Instructions too long (> 4000).", nameof(instructions));
    }

    /// <summary>
    /// Executes Update.
    /// </summary>
    public void Update(string name, string slug, string instructions, string? shortDesc, string? tips, string? tags,
                       TrainingEnvironment environment, ExerciseModality modality, DifficultyLevel difficulty,
                       MeasurementType measurementType, Visibility visibility)
    {
        ValidateCtorParams(CreatedByUserId, name, slug, instructions);
        Name = name; Slug = slug; Instructions = instructions;
        ShortDescription = shortDesc; Tips = tips; Tags = tags;
        Environment = environment; Modality = modality; DifficultyLevel = difficulty;
        MeasurementType = measurementType; Visibility = visibility;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (Status == ExerciseStatus.Approved)
        {
            Status = ExerciseStatus.Draft;
            ApprovedAt = null; ApprovedByUserId = null; ApprovedByUser = null;
        }
    }

    /// <summary>
    /// Executes Submit For Approval.
    /// </summary>
    public void SubmitForApproval()
    {
        if (Status == ExerciseStatus.Approved)
            throw new InvalidOperationException("ExercÃ­cio jÃ¡ estÃ¡ aprovado.");
        if (Status == ExerciseStatus.AwaitingApproval)
            throw new InvalidOperationException("ExercÃ­cio jÃ¡ estÃ¡ aguardando aprovaÃ§Ã£o.");
        Status = ExerciseStatus.AwaitingApproval;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Approve.
    /// </summary>
    public void Approve(Guid adminUserId)
    {
        if (Status != ExerciseStatus.AwaitingApproval && Status != ExerciseStatus.Rejected)
            throw new InvalidOperationException("ExercÃ­cio nÃ£o estÃ¡ em AwaitingApproval ou Rejected.");
        if (adminUserId == Guid.Empty) throw new ArgumentException(nameof(adminUserId));
        Status = ExerciseStatus.Approved;
        ApprovedByUserId = adminUserId;
        ApprovedAt = DateTimeOffset.UtcNow;
        RejectReason = null; RejectedAt = null; RejectedByUserId = null;
        Visibility = Visibility.Public;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Reject.
    /// </summary>
    public void Reject(Guid adminUserId, string reason)
    {
        if (Status != ExerciseStatus.AwaitingApproval)
            throw new InvalidOperationException("ExercÃ­cio nÃ£o estÃ¡ AwaitingApproval.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reject reason cannot be empty.", nameof(reason));
        if (adminUserId == Guid.Empty) throw new ArgumentException(nameof(adminUserId));
        if (reason.Length > 1000) throw new ArgumentException("Reason too long (>1000 chars).", nameof(reason));

        Status = ExerciseStatus.Rejected;
        RejectedByUserId = adminUserId;
        RejectedAt = DateTimeOffset.UtcNow;
        RejectReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Archive.
    /// </summary>
    public void Archive()
    {
        Status = ExerciseStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Muscle.
    /// </summary>
    public void AddMuscle(Muscle muscle, MuscleRole role = MuscleRole.Primary, decimal? activationPercent = null)
    {
        if (_muscles.Any(m => m.Muscle == muscle))
            throw new InvalidOperationException($"MÃºsculo {muscle} jÃ¡ adicionado neste exercÃ­cio.");
        _muscles.Add(new ExerciseMuscle(Id, muscle, role, activationPercent));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Remove Muscle.
    /// </summary>
    public void RemoveMuscle(Muscle muscle)
    {
        var m = _muscles.FirstOrDefault(x => x.Muscle == muscle);
        if (m == null) return;
        _muscles.Remove(m);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Equipment.
    /// </summary>
    public void AddEquipment(Equipment equipment, bool required = true)
    {
        if (_equipments.Any(e => e.Equipment == equipment))
            throw new InvalidOperationException($"Equipment {equipment} jÃ¡ adicionado.");
        _equipments.Add(new ExerciseEquipment(Id, equipment, required));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Remove Equipment.
    /// </summary>
    public void RemoveEquipment(Equipment equipment)
    {
        var eq = _equipments.FirstOrDefault(x => x.Equipment == equipment);
        if (eq == null) return;
        _equipments.Remove(eq);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Media.
    /// </summary>
    public Guid AddMedia(string fileId, string fileName, Shared.Enums.MediaType mediaType, int order, string? caption = null, bool isPrimary = false, long sizeBytes = 0)
    {
        var media = new ExerciseMedia(Id, fileId, fileName, mediaType, order, caption, isPrimary, sizeBytes);
        _medias.Add(media);
        if (isPrimary)
        {
            foreach (var other in _medias.Where(x => x.Id != media.Id && x.IsPrimary)) other.SetPrimary(false);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
        return media.Id;
    }

    /// <summary>
    /// Executes Remove Media.
    /// </summary>
    public void RemoveMedia(Guid mediaId)
    {
        var m = _medias.FirstOrDefault(x => x.Id == mediaId);
        if (m is null) return;
        _medias.Remove(m);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Set Primary Media.
    /// </summary>
    public void SetPrimaryMedia(Guid mediaId)
    {
        foreach (var m in _medias) m.SetPrimary(m.Id == mediaId);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Reorder Media.
    /// </summary>
    public void ReorderMedia(Guid mediaId, int newOrder)
    {
        var m = _medias.FirstOrDefault(x => x.Id == mediaId);
        if (m is null) return;
        m.SetOrder(newOrder);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

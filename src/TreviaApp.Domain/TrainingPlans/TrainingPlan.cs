using TreviaApp.Domain.Abstractions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Domain.TrainingPlans;

/// <summary>
/// Represents the TrainingPlan domain entity.
/// </summary>
public class TrainingPlan : AggregateRoot
{
    /// <summary>
    /// Gets Name.
    /// </summary>
    public string Name { get; private set; } = null!;
    /// <summary>
    /// Gets Description.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// Gets Instructions Intro.
    /// </summary>
    public string? InstructionsIntro { get; private set; }
    /// <summary>
    /// Gets Notes For Student.
    /// </summary>
    public string? NotesForStudent { get; private set; }
    /// <summary>
    /// Gets Split Type.
    /// </summary>
    public TrainingSplitType SplitType { get; private set; } = TrainingSplitType.Custom;
    /// <summary>
    /// Gets Status.
    /// </summary>
    public TrainingPlanStatus Status { get; private set; } = TrainingPlanStatus.Draft;
    /// <summary>
    /// Gets Visibility.
    /// </summary>
    public Visibility Visibility { get; private set; } = Visibility.Private;
    /// <summary>
    /// Gets Total Weeks.
    /// </summary>
    public int? TotalWeeks { get; private set; }
    /// <summary>
    /// Gets Sessions Per Week.
    /// </summary>
    public int? SessionsPerWeek { get; private set; }
    /// <summary>
    /// Gets Target Volume.
    /// </summary>
    public decimal? TargetVolume { get; private set; }
    /// <summary>
    /// Gets Tags.
    /// </summary>
    public string? Tags { get; private set; }
    /// <summary>
    /// Gets Version.
    /// </summary>
    public int Version { get; private set; } = 1;
    /// <summary>
    /// Gets Is Public Template.
    /// </summary>
    public bool IsPublicTemplate { get; private set; }

    /// <summary>
    /// Gets Created By User Id.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }
    /// <summary>
    /// Gets Created By User.
    /// </summary>
    public AppUser CreatedByUser { get; private set; } = null!;
    /// <summary>
    /// Gets Published At.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; private set; }
    /// <summary>
    /// Gets Assigned At.
    /// </summary>
    public DateTimeOffset? AssignedAt { get; private set; }
    /// <summary>
    /// Gets Completed At.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>
    /// Gets Assigned To Student Id.
    /// </summary>
    public Guid? AssignedToStudentId { get; private set; }
    /// <summary>
    /// Gets Assigned To Student.
    /// </summary>
    public AppUser? AssignedToStudent { get; private set; }

    private readonly List<TrainingSession> _sessions = new();
    /// <summary>
    /// Gets Sessions.
    /// </summary>
    public IReadOnlyCollection<TrainingSession> Sessions => _sessions.AsReadOnly();

    private TrainingPlan() { }

    /// <summary>
    /// Initializes a new instance of the TrainingPlan class.
    /// </summary>
    public TrainingPlan(
        Guid createdByUserId,
        string name,
        TrainingSplitType splitType = TrainingSplitType.Custom,
        TrainingPlanStatus status = TrainingPlanStatus.Draft,
        Visibility visibility = Visibility.Private)
    {
        ValidateBasicParams(createdByUserId, name);

        CreatedByUserId = createdByUserId;
        Name = name;
        SplitType = splitType;
        Status = status;
        Visibility = visibility;
        Version = 1;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdateIsPublicTemplate();
    }

    private static void ValidateBasicParams(Guid createdByUserId, string name)
    {
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId cannot be empty.", nameof(createdByUserId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Name too long (> 200).", nameof(name));
    }

    /// <summary>
    /// Executes Update Basic Info.
    /// </summary>
    public void UpdateBasicInfo(
        string name,
        string? description,
        string? instructionsIntro,
        string? notesForStudent,
        string? tags,
        TrainingSplitType splitType,
        Visibility visibility,
        int? totalWeeks,
        int? sessionsPerWeek)
    {
        ValidateBasicParams(CreatedByUserId, name);
        if (description != null && description.Length > 1000)
            throw new ArgumentException("Description too long (> 1000).", nameof(description));
        if (instructionsIntro != null && instructionsIntro.Length > 2000)
            throw new ArgumentException("InstructionsIntro too long (> 2000).", nameof(instructionsIntro));
        if (notesForStudent != null && notesForStudent.Length > 2000)
            throw new ArgumentException("NotesForStudent too long (> 2000).", nameof(notesForStudent));
        if (tags != null && tags.Length > 500)
            throw new ArgumentException("Tags too long (> 500).", nameof(tags));

        Name = name;
        Description = description;
        InstructionsIntro = instructionsIntro;
        NotesForStudent = notesForStudent;
        Tags = tags;
        SplitType = splitType;
        Visibility = visibility;
        TotalWeeks = totalWeeks;
        SessionsPerWeek = sessionsPerWeek;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdateIsPublicTemplate();
    }

    /// <summary>
    /// Executes Add Session.
    /// </summary>
    public Guid AddSession(
        string name,
        string? description = null,
        int? order = null,
        DayOfWeek? suggestedDayOfWeek = null,
        TimeSpan? estimatedDuration = null)
    {
        int nextOrder = order ?? (_sessions.Count == 0 ? 1 : _sessions.Max(s => s.Order) + 1);

        var session = new TrainingSession(
            Id,
            name,
            nextOrder,
            description,
            suggestedDayOfWeek,
            estimatedDuration);

        _sessions.Add(session);
        UpdatedAt = DateTimeOffset.UtcNow;
        return session.Id;
    }

    /// <summary>
    /// Executes Remove Session.
    /// </summary>
    public void RemoveSession(Guid sessionId)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session == null) return;
        _sessions.Remove(session);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Update Session.
    /// </summary>
    public void UpdateSession(
        Guid sessionId,
        string name,
        string? description,
        int order,
        DayOfWeek? suggestedDayOfWeek,
        TimeSpan? estimatedDurationMin,
        string? coachNotesInternal,
        string? focus)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session == null)
            throw new InvalidOperationException($"TrainingSession {sessionId} not found.");

        session.Update(
            name,
            order,
            description,
            suggestedDayOfWeek,
            estimatedDurationMin,
            coachNotesInternal,
            focus);

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Reorder Sessions.
    /// </summary>
    public void ReorderSessions(Dictionary<Guid, int> sessionOrders)
    {
        if (sessionOrders == null)
            throw new ArgumentNullException(nameof(sessionOrders));

        var usedNumbers = new HashSet<int>();
        foreach (var kvp in sessionOrders)
        {
            if (!usedNumbers.Add(kvp.Value))
                throw new InvalidOperationException($"Duplicate order number {kvp.Value} in reorder.");

            var session = _sessions.FirstOrDefault(s => s.Id == kvp.Key);
            if (session != null)
                session.SetOrder(kvp.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Exercise To Session.
    /// </summary>
    public Guid AddExerciseToSession(
        Guid sessionId,
        Guid exerciseId,
        int order,
        string? notesForStudent = null)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session == null)
            throw new InvalidOperationException($"TrainingSession {sessionId} not found.");

        var sessionExerciseId = session.AddExercise(exerciseId, order, notesForStudent, notesForCoach: null);
        UpdatedAt = DateTimeOffset.UtcNow;
        return sessionExerciseId;
    }

    /// <summary>
    /// Executes Remove Exercise From Session.
    /// </summary>
    public void RemoveExerciseFromSession(Guid sessionId, Guid sessionExerciseId)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session == null) return;

        session.RemoveExercise(sessionExerciseId);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Reorder Exercises In Session.
    /// </summary>
    public void ReorderExercisesInSession(Guid sessionId, Dictionary<Guid, int> exerciseOrders)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
        if (session == null)
            throw new InvalidOperationException($"TrainingSession {sessionId} not found.");

        session.ReorderExercises(exerciseOrders);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Add Prescription Sets.
    /// </summary>
    public void AddPrescriptionSets(Guid sessionExerciseId, List<(
        int setNumber,
        int? targetRepsMin,
        int? targetRepsMax,
        decimal? loadValue,
        PrescriptionLoadUnit loadUnit,
        TimeSpan? restSeconds,
        SetTechnique? technique,
        int? tempoTutSeconds,
        string? notes)> sets)
    {
        foreach (var session in _sessions)
        {
            var sessionExercise = session.FindSessionExercise(sessionExerciseId);
            if (sessionExercise == null) continue;

            foreach (var s in sets)
            {
                sessionExercise.AddPrescriptionSet(
                    s.setNumber,
                    s.targetRepsMin,
                    s.targetRepsMax,
                    s.loadValue,
                    s.loadUnit,
                    s.restSeconds,
                    s.technique,
                    s.tempoTutSeconds,
                    s.notes);
            }

            UpdatedAt = DateTimeOffset.UtcNow;
            return;
        }

        throw new InvalidOperationException($"SessionExercise {sessionExerciseId} not found in this plan.");
    }

    /// <summary>
    /// Executes Publish.
    /// </summary>
    public void Publish()
    {
        Status = TrainingPlanStatus.Published;
        if (Visibility != Visibility.Public)
            Visibility = Visibility.Public;
        PublishedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdateIsPublicTemplate();
    }

    /// <summary>
    /// Executes Unpublish.
    /// </summary>
    public void Unpublish()
    {
        Status = TrainingPlanStatus.Draft;
        PublishedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdateIsPublicTemplate();
    }

    /// <summary>
    /// Executes Assign To Student.
    /// </summary>
    public void AssignToStudent(Guid studentId)
    {
        if (studentId == Guid.Empty)
            throw new ArgumentException("StudentId cannot be empty.", nameof(studentId));
        if (AssignedToStudentId.HasValue)
            throw new InvalidOperationException("This training plan is already assigned to a student.");

        AssignedToStudentId = studentId;
        Status = TrainingPlanStatus.Active;
        AssignedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Pause.
    /// </summary>
    public void Pause()
    {
        if (Status != TrainingPlanStatus.Active)
            throw new InvalidOperationException($"Can only pause Active plans. Current status: {Status}");

        Status = TrainingPlanStatus.Paused;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Resume.
    /// </summary>
    public void Resume()
    {
        if (Status != TrainingPlanStatus.Paused)
            throw new InvalidOperationException($"Can only resume Paused plans. Current status: {Status}");

        Status = TrainingPlanStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Complete.
    /// </summary>
    public void Complete()
    {
        Status = TrainingPlanStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Archive.
    /// </summary>
    public void Archive()
    {
        Status = TrainingPlanStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Executes Duplicate.
    /// </summary>
    public TrainingPlan Duplicate(Guid newOwnerUserId, bool keepStatusDraft = true)
    {
        if (newOwnerUserId == Guid.Empty)
            throw new ArgumentException("NewOwnerUserId cannot be empty.", nameof(newOwnerUserId));

        var copy = new TrainingPlan(
            newOwnerUserId,
            Name,
            SplitType,
            keepStatusDraft ? TrainingPlanStatus.Draft : Status,
            keepStatusDraft ? Visibility.Private : Visibility)
        {
            Description = Description,
            InstructionsIntro = InstructionsIntro,
            NotesForStudent = NotesForStudent,
            TotalWeeks = TotalWeeks,
            SessionsPerWeek = SessionsPerWeek,
            TargetVolume = TargetVolume,
            Tags = Tags,
            Version = Version + 1,
            PublishedAt = keepStatusDraft ? null : PublishedAt,
            AssignedAt = null,
            CompletedAt = null,
            AssignedToStudentId = null
        };

        copy.UpdateIsPublicTemplate();

        var copiedSessions = new List<TrainingSession>();
        foreach (var originalSession in _sessions.OrderBy(s => s.Order))
        {
            var copiedSession = new TrainingSession(
                copy.Id,
                originalSession.Name,
                originalSession.Order,
                originalSession.Description,
                originalSession.SuggestedDayOfWeek,
                originalSession.EstimatedDurationMin,
                originalSession.CoachNotesInternal,
                originalSession.Focus);

            var copiedExercises = new List<SessionExercise>();
            foreach (var originalExercise in originalSession.Exercises.OrderBy(e => e.Order))
            {
                var copiedExercise = new SessionExercise(
                    copiedSession.Id,
                    originalExercise.ExerciseId,
                    originalExercise.Order,
                    originalExercise.NotesForStudent,
                    originalExercise.NotesForCoach,
                    originalExercise.RestBetweenSetsSeconds,
                    originalExercise.GlobalSetTechniqueAppliedToAllSets,
                    originalExercise.GlobalLoadOverrideKg,
                    originalExercise.GlobalRepsOverride);

                var copiedSets = new List<SetPrescription>();
                foreach (var originalSet in originalExercise.Prescriptions.OrderBy(p => p.SetNumber))
                {
                    var copiedSet = new SetPrescription(
                        copiedExercise.Id,
                        originalSet.SetNumber,
                        originalSet.TargetRepsMin,
                        originalSet.TargetRepsMax,
                        originalSet.LoadValue,
                        originalSet.LoadUnit,
                        originalSet.RestAfterSeconds,
                        originalSet.TechniqueApplied,
                        originalSet.RateOfPerceivedExertionRPE,
                        originalSet.RepsInReserveRIR,
                        originalSet.TempoUnderTensionTUTSeconds,
                        originalSet.NotesProfessor,
                        originalSet.TempoNotation);

                    copiedSets.Add(copiedSet);
                }

                copiedExercise.ImportPrescriptions(copiedSets);
                copiedExercises.Add(copiedExercise);
            }

            copiedSession.ImportExercises(copiedExercises);
            copiedSessions.Add(copiedSession);
        }

        foreach (var s in copiedSessions)
            copy._sessions.Add(s);

        copy.UpdatedAt = DateTimeOffset.UtcNow;
        return copy;
    }

    private void UpdateIsPublicTemplate()
    {
        IsPublicTemplate = Visibility == Visibility.Public && Status == TrainingPlanStatus.Published;
    }
}

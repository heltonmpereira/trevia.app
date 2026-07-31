namespace TreviaApp.Application.TrainingPlans.Mappings;

using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.Domain.TrainingPlans;

public static class TrainingPlanMappings
{
    public static TrainingPlanSummaryResponse MapToSummary(
        TrainingPlan tp,
        int totalSessions,
        int totalExercises,
        string? createdByName = null,
        string? assignedToStudentName = null)
    {
        return new TrainingPlanSummaryResponse(
            tp.Id,
            tp.Name,
            tp.Description,
            tp.SplitType,
            tp.Status,
            tp.Visibility,
            tp.IsPublicTemplate,
            tp.Version,
            totalSessions,
            totalExercises,
            tp.TotalWeeks,
            tp.SessionsPerWeek,
            tp.CreatedAt,
            tp.UpdatedAt,
            createdByName,
            tp.AssignedToStudentId,
            assignedToStudentName,
            tp.AssignedAt);
    }

    public static TrainingPlanDetailResponse MapToDetail(
        TrainingPlan tp,
        string? createdByName,
        string? assignedToStudentName,
        bool hideCoachNotes = false)
    {
        var sessions = tp.Sessions
            .OrderBy(s => s.Order)
            .Select(s => MapSession(s, hideCoachNotes))
            .ToList();

        return new TrainingPlanDetailResponse(
            tp.Id,
            tp.Name,
            tp.Description,
            tp.InstructionsIntro,
            tp.NotesForStudent,
            tp.SplitType,
            tp.Status,
            tp.Visibility,
            tp.IsPublicTemplate,
            tp.Version,
            tp.TotalWeeks,
            tp.SessionsPerWeek,
            tp.TargetVolume,
            tp.Tags,
            tp.CreatedAt,
            tp.UpdatedAt,
            tp.CreatedByUserId,
            createdByName,
            tp.PublishedAt,
            tp.AssignedAt,
            tp.CompletedAt,
            tp.AssignedToStudentId,
            assignedToStudentName,
            sessions);
    }

    public static TrainingSessionResponse MapSession(TrainingSession s, bool hideCoachNotes = false)
    {
        var exercises = s.Exercises
            .OrderBy(e => e.Order)
            .Select(e => MapSessionExercise(e, hideCoachNotes))
            .ToList();

        return new TrainingSessionResponse(
            s.Id,
            s.Name,
            s.Order,
            s.Description,
            s.SuggestedDayOfWeek,
            s.EstimatedDurationMin,
            hideCoachNotes ? null : s.CoachNotesInternal,
            s.Focus,
            exercises);
    }

    public static SessionExerciseResponse MapSessionExercise(SessionExercise se, bool hideCoachNotes)
    {
        string? exerciseName = null;
        string? exerciseShortDesc = null;
        if (se.Exercise != null)
        {
            exerciseName = se.Exercise.Name;
            exerciseShortDesc = se.Exercise.ShortDescription;
        }

        var prescriptions = se.Prescriptions
            .OrderBy(p => p.SetNumber)
            .Select(MapSetPrescription)
            .ToList();

        return new SessionExerciseResponse(
            se.Id,
            se.Order,
            se.ExerciseId,
            exerciseName ?? string.Empty,
            exerciseShortDesc,
            null,
            se.NotesForStudent,
            hideCoachNotes ? null : se.NotesForCoach,
            se.RestBetweenSetsSeconds,
            se.GlobalSetTechniqueAppliedToAllSets,
            prescriptions);
    }

    public static SetPrescriptionResponse MapSetPrescription(SetPrescription p)
    {
        return new SetPrescriptionResponse(
            p.Id,
            p.SetNumber,
            p.TargetRepsMin,
            p.TargetRepsMax,
            p.LoadValue,
            p.LoadUnit,
            p.RestAfterSeconds,
            p.TechniqueApplied,
            p.RateOfPerceivedExertionRPE,
            p.RepsInReserveRIR,
            p.TempoUnderTensionTUTSeconds,
            p.NotesProfessor,
            p.TempoNotation);
    }
}

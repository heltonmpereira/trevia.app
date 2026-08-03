using TreviaApp.Domain.WorkoutExecution;

namespace TreviaApp.UnitTests.WorkoutExecution;

public class WorkoutSessionDurationTests
{
    private static WorkoutSession CreateSession()
    {
        return new WorkoutSession(
            studentId: Guid.NewGuid(),
            trainingPlanId: null,
            trainingSessionId: null,
            name: "Treino de Teste");
    }

    [Fact]
    public void Constructor_SetsStatusNotStarted()
    {
        var ws = CreateSession();
        ws.Status.Should().Be(WorkoutStatus.NotStarted);
        ws.StartedAt.Should().BeNull();
        ws.FinishedAt.Should().BeNull();
        ws.TotalDurationElapsed.Should().BeNull();
        ws.ActiveTime.Should().BeNull();
    }

    [Fact]
    public void Start_SetsInProgressAndStartedAt()
    {
        var ws = CreateSession();
        ws.Start();

        ws.Status.Should().Be(WorkoutStatus.InProgress);
        ws.StartedAt.Should().NotBeNull();
        ws.StartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Finish_WithoutStart_Throws()
    {
        var ws = CreateSession();
        var act = () => ws.Finish();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Finish_AfterStart_SetsCompletedAndActiveTime()
    {
        var ws = CreateSession();
        ws.Start();

        ws.Finish(overallRating: WorkoutRating.Moderate);

        ws.Status.Should().Be(WorkoutStatus.Completed);
        ws.FinishedAt.Should().NotBeNull();
        ws.ActiveTime.Should().NotBeNull();
        ws.ActiveTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        ws.TotalDurationElapsed.Should().NotBeNull();
    }

    [Fact]
    public void Finish_WithInterruptedRating_SetsInterruptedStatus()
    {
        var ws = CreateSession();
        ws.Start();

        ws.Finish(overallRating: WorkoutRating.Interrupted);

        ws.Status.Should().Be(WorkoutStatus.Interrupted);
    }

    [Fact]
    public void Pause_WhenInProgress_SetsPaused()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Pause();

        ws.Status.Should().Be(WorkoutStatus.Paused);
        ws.Pauses.Should().HaveCount(1);
    }

    [Fact]
    public void Pause_WhenNotStarted_Throws()
    {
        var ws = CreateSession();
        var act = () => ws.Pause();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pause_WhenAlreadyPaused_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Pause();

        var act = () => ws.Pause();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resume_WhenPaused_ReturnsToInProgress()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Pause();
        ws.Resume();

        ws.Status.Should().Be(WorkoutStatus.InProgress);
    }

    [Fact]
    public void Resume_WhenInProgress_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        var act = () => ws.Resume();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PauseDuration_NotCountedInActiveTime()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Pause();

        System.Threading.Thread.Sleep(2000);

        ws.Resume();
        ws.Finish();

        var totalElapsed = ws.TotalDurationElapsed!.Value;
        var activeTime = ws.ActiveTime!.Value;
        var diff = totalElapsed - activeTime;
        diff.TotalSeconds.Should().BeGreaterThanOrEqualTo(1.5);
    }

    [Fact]
    public void Finish_AlreadyCompleted_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Finish();

        var act = () => ws.Finish();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Start_AfterCompleted_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Finish();

        var act = () => ws.Start();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Finish_NegativeCalories_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        var act = () => ws.Finish(caloriesBurned: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Finish_LongNotes_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        string longNotes = new string('N', 2001);
        var act = () => ws.Finish(generalNotes: longNotes);

        act.Should().Throw<ArgumentException>().WithMessage("*GeneralNotes too long*");
    }

    [Fact]
    public void Finish_FromPausedStatus_Allowed()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Pause();

        ws.Finish(overallRating: WorkoutRating.Moderate);
        ws.Status.Should().Be(WorkoutStatus.Completed);
    }

    [Fact]
    public void AddExercisesFromPrescription_WhenNotStarted_AddsExercises()
    {
        var ws = CreateSession();
        var exerciseId = Guid.NewGuid();
        var sessionExerciseId = Guid.NewGuid();

        ws.AddExercisesFromPrescription(new[]
        {
            (sessionExerciseId, exerciseId, 1, (string?)"Notas do aluno")
        });

        ws.Exercises.Should().HaveCount(1);
        ws.Exercises.First().ExerciseId.Should().Be(exerciseId);
    }

    [Fact]
    public void AddExercisesFromPrescription_WhenFinished_Throws()
    {
        var ws = CreateSession();
        ws.Start();
        ws.Finish();

        var act = () => ws.AddExercisesFromPrescription(Array.Empty<(Guid, Guid, int, string?)>());
        act.Should().Throw<InvalidOperationException>();
    }
}

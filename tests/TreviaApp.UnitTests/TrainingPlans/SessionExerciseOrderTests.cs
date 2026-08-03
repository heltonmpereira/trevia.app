using TreviaApp.Domain.TrainingPlans;

namespace TreviaApp.UnitTests.TrainingPlans;

public class SessionExerciseOrderTests
{
    private static SessionExercise CreateExercise(int order)
    {
        return new SessionExercise(
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: order);
    }

    [Fact]
    public void Constructor_OrderLessThan1_Throws()
    {
        var act = () => new SessionExercise(
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ValidOrder_SetsCorrectly()
    {
        var ex = CreateExercise(3);
        ex.Order.Should().Be(3);
    }

    [Fact]
    public void SetOrder_ValidValue_Updates()
    {
        var ex = CreateExercise(5);
        ex.SetOrder(10);
        ex.Order.Should().Be(10);
    }

    [Fact]
    public void SetOrder_InvalidValue_Throws()
    {
        var ex = CreateExercise(5);
        var act = () => ex.SetOrder(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateBasicInfo_ValidOrder_UpdatesAllFields()
    {
        var ex = CreateExercise(1);
        var tsId = ex.TrainingSessionId;
        var eId = ex.ExerciseId;

        ex.UpdateBasicInfo(
            order: 7,
            notesForStudent: "Foco na técnica",
            notesForCoach: "Aumentar carga",
            restBetweenSetsSeconds: TimeSpan.FromSeconds(120),
            globalSetTechniqueAppliedToAllSets: SetTechnique.DropSet,
            globalLoadOverrideKg: 40,
            globalRepsOverride: 10);

        ex.TrainingSessionId.Should().Be(tsId);
        ex.ExerciseId.Should().Be(eId);
        ex.Order.Should().Be(7);
        ex.NotesForStudent.Should().Be("Foco na técnica");
        ex.NotesForCoach.Should().Be("Aumentar carga");
        ex.RestBetweenSetsSeconds.Should().Be(TimeSpan.FromSeconds(120));
        ex.GlobalSetTechniqueAppliedToAllSets.Should().Be(SetTechnique.DropSet);
        ex.GlobalLoadOverrideKg.Should().Be(40);
        ex.GlobalRepsOverride.Should().Be(10);
    }

    [Fact]
    public void UpdateBasicInfo_InvalidOrder_Throws()
    {
        var ex = CreateExercise(1);
        var act = () => ex.UpdateBasicInfo(
            order: -1,
            notesForStudent: null,
            notesForCoach: null,
            restBetweenSetsSeconds: null,
            globalSetTechniqueAppliedToAllSets: null,
            globalLoadOverrideKg: null,
            globalRepsOverride: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddPrescriptionSet_DuplicateSetNumber_Throws()
    {
        var ex = CreateExercise(1);
        ex.AddPrescriptionSet(setNumber: 1, targetRepsMin: 10);

        var act = () => ex.AddPrescriptionSet(setNumber: 1, targetRepsMin: 8);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Set number 1 already exists*");
    }

    [Fact]
    public void AddPrescriptionSet_Valid_AddsToPrescriptions()
    {
        var ex = CreateExercise(1);
        var id = ex.AddPrescriptionSet(setNumber: 1, targetRepsMin: 8, targetRepsMax: 12, loadValue: 20);

        ex.Prescriptions.Should().HaveCount(1);
        var set = ex.Prescriptions.First();
        set.Id.Should().Be(id);
        set.SetNumber.Should().Be(1);
    }

    [Fact]
    public void ReorderSets_Swap1And3_OrderCorrectlyUpdated()
    {
        var ex = CreateExercise(1);
        var id1 = ex.AddPrescriptionSet(setNumber: 1, targetRepsMin: 10);
        var id2 = ex.AddPrescriptionSet(setNumber: 2, targetRepsMin: 10);
        var id3 = ex.AddPrescriptionSet(setNumber: 3, targetRepsMin: 10);

        var mapping = new Dictionary<Guid, int>
        {
            { id1, 3 },
            { id3, 1 },
            { id2, 2 }
        };
        ex.ReorderSets(mapping);

        ex.Prescriptions.First(p => p.Id == id1).SetNumber.Should().Be(3);
        ex.Prescriptions.First(p => p.Id == id2).SetNumber.Should().Be(2);
        ex.Prescriptions.First(p => p.Id == id3).SetNumber.Should().Be(1);
    }

    [Fact]
    public void ReorderSets_DuplicateNumber_Throws()
    {
        var ex = CreateExercise(1);
        var id1 = ex.AddPrescriptionSet(setNumber: 1);
        var id2 = ex.AddPrescriptionSet(setNumber: 2);

        var act = () => ex.ReorderSets(new Dictionary<Guid, int>
        {
            { id1, 1 },
            { id2, 1 }
        });

        act.Should().Throw<InvalidOperationException>().WithMessage("*Duplicate set number*");
    }

    [Fact]
    public void ReorderSets_NullDict_Throws()
    {
        var ex = CreateExercise(1);
        var act = () => ex.ReorderSets(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemovePrescriptionSet_Existing_Removes()
    {
        var ex = CreateExercise(1);
        var id1 = ex.AddPrescriptionSet(setNumber: 1);
        ex.AddPrescriptionSet(setNumber: 2);

        ex.RemovePrescriptionSet(id1);

        ex.Prescriptions.Should().HaveCount(1);
        ex.Prescriptions.Single().SetNumber.Should().Be(2);
    }

    [Fact]
    public void RemovePrescriptionSet_NonExisting_NoOp()
    {
        var ex = CreateExercise(1);
        ex.AddPrescriptionSet(setNumber: 1);
        ex.RemovePrescriptionSet(Guid.NewGuid());
        ex.Prescriptions.Should().HaveCount(1);
    }

    [Fact]
    public void NotesForStudent_TooLong_Throws()
    {
        var act = () => new SessionExercise(
            trainingSessionId: Guid.NewGuid(),
            exerciseId: Guid.NewGuid(),
            order: 1,
            notesForStudent: new string('A', 1001));

        act.Should().Throw<ArgumentException>().WithMessage("*NotesForStudent too long*");
    }
}

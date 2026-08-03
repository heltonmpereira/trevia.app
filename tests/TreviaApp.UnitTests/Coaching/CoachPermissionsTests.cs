using TreviaApp.Domain.Coaching;

namespace TreviaApp.UnitTests.Coaching;

public class CoachPermissionsTests
{
    private static CoachStudentLink CreateLink(
        CoachPermissions? permissions = null,
        Guid? coachId = null,
        Guid? studentId = null)
    {
        return new CoachStudentLink(
            coachId: coachId ?? Guid.NewGuid(),
            studentId: studentId ?? Guid.NewGuid(),
            initialPermissions: permissions);
    }

    [Fact]
    public void Constructor_DefaultPermissions_IncludesAllBasic()
    {
        var link = CreateLink();
        link.IsActive.Should().BeTrue();
        link.HasPermission(CoachPermissions.CanViewWeightHistory).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanViewBodyMeasurements).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanViewProfilePhotos).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanAssignTrainingPlans).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanViewWorkoutHistory).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanSendFeedback).Should().BeFalse();
    }

    [Fact]
    public void Constructor_CustomPermissions_Applied()
    {
        var custom = CoachPermissions.CanViewWorkoutHistory | CoachPermissions.CanSendFeedback;
        var link = CreateLink(custom);

        link.HasPermission(CoachPermissions.CanViewWorkoutHistory).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanSendFeedback).Should().BeTrue();
        link.HasPermission(CoachPermissions.CanViewWeightHistory).Should().BeFalse();
    }

    [Fact]
    public void Constructor_SameCoachAndStudent_Throws()
    {
        var id = Guid.NewGuid();
        var act = () => new CoachStudentLink(id, id);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Coach and student cannot be the same*");
    }

    [Fact]
    public void Constructor_EmptyCoach_Throws()
    {
        var act = () => new CoachStudentLink(Guid.Empty, Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EmptyStudent_Throws()
    {
        var act = () => new CoachStudentLink(Guid.NewGuid(), Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HasPermission_NonePerm_ReturnsFalseForNonZero()
    {
        var link = CreateLink(CoachPermissions.None);
        link.HasPermission(CoachPermissions.CanViewWeightHistory).Should().BeFalse();
        link.HasPermission(CoachPermissions.CanAssignTrainingPlans).Should().BeFalse();
    }

    [Fact]
    public void GrantPermission_ByCoach_Added()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(CoachPermissions.None, coachId: coachId);
        link.HasPermission(CoachPermissions.CanSendFeedback).Should().BeFalse();

        link.GrantPermission(CoachPermissions.CanSendFeedback, coachId);

        link.HasPermission(CoachPermissions.CanSendFeedback).Should().BeTrue();
    }

    [Fact]
    public void GrantPermission_ByNonCoach_Throws()
    {
        var coachId = Guid.NewGuid();
        var other = Guid.NewGuid();
        var link = CreateLink(CoachPermissions.None, coachId: coachId);

        var act = () => link.GrantPermission(CoachPermissions.CanSendFeedback, other);
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void RevokePermission_ByCoach_Removed()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(CoachPermissions.CanViewWeightHistory | CoachPermissions.CanViewWorkoutHistory, coachId: coachId);

        link.RevokePermission(CoachPermissions.CanViewWeightHistory, coachId);

        link.HasPermission(CoachPermissions.CanViewWeightHistory).Should().BeFalse();
        link.HasPermission(CoachPermissions.CanViewWorkoutHistory).Should().BeTrue();
    }

    [Fact]
    public void RevokePermission_ByNonCoach_Throws()
    {
        var coachId = Guid.NewGuid();
        var other = Guid.NewGuid();
        var link = CreateLink(coachId: coachId);

        var act = () => link.RevokePermission(CoachPermissions.CanViewWeightHistory, other);
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void UpdatePermissions_ByCoach_ReplacesAll()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(CoachPermissions.CanViewWeightHistory, coachId: coachId);

        link.UpdatePermissions(CoachPermissions.CanInviteToGroups, coachId);

        link.Permissions.Should().Be(CoachPermissions.CanInviteToGroups);
        link.HasPermission(CoachPermissions.CanViewWeightHistory).Should().BeFalse();
    }

    [Fact]
    public void UpdatePermissions_InactiveLink_Throws()
    {
        var coachId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId, studentId: studentId);
        link.EndRelationship(coachId, CoachRelationshipEndReason.MutualAgreement);

        var act = () => link.UpdatePermissions(CoachPermissions.None, coachId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
    }

    [Fact]
    public void GrantPermission_InactiveLink_Throws()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId);
        link.EndRelationship(coachId, CoachRelationshipEndReason.EndedByCoach);

        var act = () => link.GrantPermission(CoachPermissions.CanSendFeedback, coachId);
        act.Should().Throw<InvalidOperationException>().WithMessage("*inactive*");
    }

    [Fact]
    public void EndRelationship_AlreadyInactive_Throws()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId);
        link.EndRelationship(coachId, CoachRelationshipEndReason.MutualAgreement);

        var act = () => link.EndRelationship(coachId, CoachRelationshipEndReason.MutualAgreement);
        act.Should().Throw<InvalidOperationException>().WithMessage("*already inactive*");
    }

    [Fact]
    public void EndRelationship_ByThirdParty_Throws()
    {
        var coachId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId, studentId: studentId);
        var outsider = Guid.NewGuid();

        var act = () => link.EndRelationship(outsider, CoachRelationshipEndReason.Other);
        act.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void EndRelationship_ByCoach_SetsInactive()
    {
        var coachId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId, studentId: studentId);

        link.EndRelationship(coachId, CoachRelationshipEndReason.EndedByCoach, "Notas do fim");

        link.IsActive.Should().BeFalse();
        link.EndedAt.Should().NotBeNull();
        link.EndReason.Should().Be(CoachRelationshipEndReason.EndedByCoach);
        link.EndReasonNotes.Should().Be("Notas do fim");
    }

    [Fact]
    public void EndRelationship_ByStudent_SetsInactive()
    {
        var coachId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId, studentId: studentId);

        link.EndRelationship(studentId, CoachRelationshipEndReason.EndedByStudent);

        link.IsActive.Should().BeFalse();
        link.EndReason.Should().Be(CoachRelationshipEndReason.EndedByStudent);
    }

    [Fact]
    public void EndRelationship_LongNotes_Throws()
    {
        var coachId = Guid.NewGuid();
        var link = CreateLink(coachId: coachId);
        string longNotes = new string('N', 1001);

        var act = () => link.EndRelationship(coachId, CoachRelationshipEndReason.Other, longNotes);
        act.Should().Throw<ArgumentException>().WithMessage("*EndReasonNotes too long*");
    }

    [Fact]
    public void StartedAt_SetDuringConstructor()
    {
        var link = CreateLink();
        link.StartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Permissions_Flags_ArePowersOfTwo()
    {
        var values = Enum.GetValues<CoachPermissions>()
            .Cast<int>()
            .Where(v => v != 0)
            .ToList();

        foreach (var v in values)
        {
            (v & (v - 1)).Should().Be(0, because: $"enum value {v} should be a single flag (power of 2)");
        }
    }
}

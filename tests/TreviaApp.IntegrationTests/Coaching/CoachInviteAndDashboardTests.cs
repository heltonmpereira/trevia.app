namespace TreviaApp.IntegrationTests.Coaching;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Contracts.Coaching.Requests;
using TreviaApp.Contracts.Coaching.Responses;
using TreviaApp.IntegrationTests.Utilities;
using TreviaApp.Shared.Enums;
using Xunit;

[Collection("Auth Integration Tests")]
public class CoachInviteAndDashboardTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public CoachInviteAndDashboardTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private static string NewEmail(string prefix) =>
        $"{prefix}_{Guid.NewGuid().ToString("N")[..6]}@test.com";

    private async Task<AuthResponse> RegisterAndLogin(string email)
    {
        await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        return await _client.LoginAsync(email, "Student123!");
    }

    [Fact]
    public async Task SendCoachInvite_Student_Accepts_CoachSeesStudentInList()
    {
        var coachEmail = NewEmail("coach_invite");
        var studentEmail = NewEmail("student_invite");

        var coachAuth = await RegisterAndLogin(coachEmail);
        var studentAuth = await RegisterAndLogin(studentEmail);
        var studentId = studentAuth.UserId;
        var coachId = coachAuth.UserId;

        _client.WithBearer(coachAuth);
        var inviteReq = new SendCoachInviteRequest(
            StudentId: studentId,
            Message: "Olá! Gostaria de ser seu professor.",
            ExpiresInDays: 14,
            GrantedPermissionsOnAccept: CoachPermissions.CanViewWorkoutHistory
                | CoachPermissions.CanAssignTrainingPlans);

        var inviteResp = await _client.PostAsJsonAsync("/api/coaching/invites/send-trainer", inviteReq);
        inviteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted);
        var invite = await inviteResp.Content.ReadFromJsonAsync<CoachInviteResponse>();
        invite.Should().NotBeNull();
        var inviteId = invite!.Id;
        inviteId.Should().NotBeEmpty();

        _client.DefaultRequestHeaders.Authorization = null;
        _client.WithBearer(studentAuth);

        var incomingResp = await _client.GetFromJsonAsync<CoachingInvitesPagedResponse>(
            "/api/coaching/invites/incoming?pageSize=50");
        incomingResp.Should().NotBeNull();
        incomingResp!.Items.Should().Contain(i => i.Id == inviteId);

        var acceptResp = await _client.PostAsJsonAsync($"/api/coaching/invites/{inviteId}/accept",
            new AcceptCoachInviteRequest(AcceptanceNote: "Top, bora!"));
        acceptResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.NoContent);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.WithBearer(coachAuth);
        var studentsResp = await _client.GetFromJsonAsync<CoachStudentsPagedResponse>(
            "/api/coaching/students?pageSize=50");
        studentsResp.Should().NotBeNull();
        studentsResp!.Items.Should().Contain(s => s.StudentId == studentId);
    }

    [Fact]
    public async Task SendCoachInvite_WithoutAuth_ReturnsUnauthorized()
    {
        var req = new SendCoachInviteRequest(Guid.NewGuid(), "msg");
        var resp = await _client.PostAsJsonAsync("/api/coaching/invites/send-trainer", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectCoachInvite_Works()
    {
        var coachEmail = NewEmail("coach_rej");
        var studentEmail = NewEmail("student_rej");
        var coachAuth = await RegisterAndLogin(coachEmail);
        var studentAuth = await RegisterAndLogin(studentEmail);
        var studentId = studentAuth.UserId;

        _client.WithBearer(coachAuth);
        var inviteResp = await _client.PostAsJsonAsync("/api/coaching/invites/send-trainer",
            new SendCoachInviteRequest(studentId));
        var invite = (await inviteResp.Content.ReadFromJsonAsync<CoachInviteResponse>())!;

        _client.DefaultRequestHeaders.Authorization = null;
        _client.WithBearer(studentAuth);
        var rejectResp = await _client.PostAsJsonAsync($"/api/coaching/invites/{invite.Id}/reject",
            new RejectCoachInviteRequest(Reason: "Não no momento"));
        rejectResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.Accepted);
    }
}

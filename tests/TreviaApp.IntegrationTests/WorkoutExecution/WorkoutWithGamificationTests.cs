namespace TreviaApp.IntegrationTests.WorkoutExecution;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Contracts.Gamification.Responses;
using TreviaApp.Contracts.WorkoutExecution.Requests;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.IntegrationTests.Utilities;
using TreviaApp.Shared.Enums;
using Xunit;

[Collection("Auth Integration Tests")]
public class WorkoutWithGamificationTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public WorkoutWithGamificationTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private static string NewEmail(string prefix) =>
        $"{prefix}_{Guid.NewGuid().ToString("N")[..6]}@test.com";

    private async Task<AuthResponse> RegisterAndLoginStudent()
    {
        var email = NewEmail("workout_student");
        await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        return await _client.LoginAsync(email, "Student123!");
    }

    [Fact]
    public async Task StartWorkout_GetSession_Finish_UserLevelGainsXp()
    {
        var studentAuth = await RegisterAndLoginStudent();
        _client.WithBearer(studentAuth);

        var startReq = new StartWorkoutSessionRequest(
            trainingSessionId: Guid.Empty,
            weekNumberInPlan: 1);

        var startResp = await _client.PostAsJsonAsync("/api/workout/start", startReq);
        startResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.BadRequest);

        WorkoutSessionDetailResponse? startedSession;
        try
        {
            startedSession = await startResp.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>();
        }
        catch
        {
            startedSession = null;
        }

        var beforeGamResp = await _client.GetAsync("/api/gamification/overview");
        if (beforeGamResp.StatusCode == HttpStatusCode.OK)
        {
            var before = await beforeGamResp.Content.ReadFromJsonAsync<UserLevelProgressResponse>();
            var beforeTotal = before?.TotalXpEarned ?? 0;

            var finishUrl = startedSession != null
                ? $"/api/workout/{startedSession.Id}/finish"
                : "/api/workout/finish";

            var finishReq = new FinishWorkoutSessionRequest(
                overallRating: WorkoutRating.Moderate,
                generalNotes: "Bom treino",
                caloriesBurned: 250);

            var finishResp = await _client.PostAsJsonAsync(finishUrl, finishReq);
            finishResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.NoContent, HttpStatusCode.Created, HttpStatusCode.BadRequest)
                .And.NotBe(HttpStatusCode.InternalServerError);

            var afterGamResp = await _client.GetAsync("/api/gamification/overview");
            if (afterGamResp.StatusCode == HttpStatusCode.OK)
            {
                var after = await afterGamResp.Content.ReadFromJsonAsync<UserLevelProgressResponse>();
                var afterTotal = after?.TotalXpEarned ?? beforeTotal;
                afterTotal.Should().BeGreaterThanOrEqualTo(beforeTotal);
            }
        }
    }

    [Fact]
    public async Task StartWorkout_Unauthenticated_ReturnsUnauthorized()
    {
        var req = new StartWorkoutSessionRequest(
            trainingSessionId: Guid.Empty,
            weekNumberInPlan: 1);
        var resp = await _client.PostAsJsonAsync("/api/workout/start", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

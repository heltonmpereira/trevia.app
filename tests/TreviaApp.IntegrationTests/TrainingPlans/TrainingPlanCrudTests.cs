namespace TreviaApp.IntegrationTests.TrainingPlans;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Contracts.TrainingPlans.Requests;
using TreviaApp.Contracts.TrainingPlans.Responses;
using TreviaApp.IntegrationTests.Utilities;
using TreviaApp.Shared.Enums;
using Xunit;

[Collection("Auth Integration Tests")]
public class TrainingPlanCrudTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public TrainingPlanCrudTests(TestWebApplicationFactory factory) => _factory = factory;

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
    public async Task Create_GetMine_Update_GetById_Delete_Works()
    {
        var trainerEmail = NewEmail("tp_trainer");
        var auth = await RegisterAndLogin(trainerEmail);
        _client.WithBearer(auth);

        var createReq = new CreateTrainingPlanRequest(
            Name: "Plano Iniciante ABC",
            Description: "Plano básico para iniciantes 3x/semana",
            InstructionsIntro: "Aquecer 5 min antes de cada sessão",
            NotesForStudent: "Foco em técnica, não carga",
            SplitType: TrainingSplitType.FullBody,
            Visibility: Visibility.Private,
            TotalWeeks: 8,
            SessionsPerWeek: 3,
            TargetVolume: null,
            Tags: "iniciante,fullbody,abc");

        var createResp = await _client.PostAsJsonAsync("/api/training-plans", createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Plano Iniciante ABC");

        var mineResp = await _client.GetFromJsonAsync<TrainingPlansSearchPagedResponse>("/api/training-plans/mine?pageSize=50");
        mineResp.Should().NotBeNull();
        mineResp!.Items.Should().Contain(i => i.Id == created.Id);

        var updateReq = new UpdateTrainingPlanRequest(
            Name: "Plano Iniciante ABC - Atualizado",
            Description: createReq.Description,
            InstructionsIntro: createReq.InstructionsIntro,
            NotesForStudent: createReq.NotesForStudent,
            SplitType: createReq.SplitType,
            Visibility: createReq.Visibility,
            TotalWeeks: 12,
            SessionsPerWeek: 3,
            TargetVolume: null,
            Tags: createReq.Tags,
            DifficultyLevel: DifficultyLevel.Beginner,
            Goal: TrainingGoal.Hypertrophy,
            Environment: TrainingEnvironment.Gym);

        var updateResp = await _client.PutAsJsonAsync($"/api/training-plans/{created.Id}", updateReq);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await _client.GetFromJsonAsync<TrainingPlanDetailResponse>($"/api/training-plans/{created.Id}");
        getResp.Should().NotBeNull();
        getResp!.Name.Should().Be("Plano Iniciante ABC - Atualizado");
        getResp.TotalWeeks.Should().Be(12);

        var deleteResp = await _client.DeleteAsync($"/api/training-plans/{created.Id}");
        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync($"/api/training-plans/{created.Id}");
        afterDelete.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddSession_AddExercise_AssignToStudent_StudentCanGetAssigned()
    {
        var trainerEmail = NewEmail("t_trainer");
        var studentEmail = NewEmail("t_student");

        var trainerAuth = await RegisterAndLogin(trainerEmail);
        var studentAuth = await RegisterAndLogin(studentEmail);
        var studentId = studentAuth.UserId;

        _client.WithBearer(trainerAuth);
        var plan = await _client.PostAsJsonAsync("/api/training-plans", new CreateTrainingPlanRequest(
            "Plano Atribuicao",
            "desc",
            "intro",
            "notes",
            TrainingSplitType.UpperLower,
            Visibility: Visibility.Private));
        var planData = (await plan.Content.ReadFromJsonAsync<TrainingPlanDetailResponse>())!;
        var planId = planData.Id;

        var addSessionReq = new AddTrainingSessionRequest(
            Name: "Dia A - Superior",
            Description: "Peito, ombro, tríceps",
            Weekday: null,
            EstimatedDurationMinutes: 60);
        var addSessionResp = await _client.PostAsJsonAsync($"/api/training-plans/{planId}/sessions", addSessionReq);
        addSessionResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var sessionData = await addSessionResp.Content.ReadFromJsonAsync<TrainingSessionResponse>();
        sessionData.Should().NotBeNull();
        var sessionId = sessionData!.Id;

        var assignResp = await _client.PutAsync($"/api/training-plans/{planId}/assign/{studentId}", null);
        assignResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.Accepted);

        _client.DefaultRequestHeaders.Authorization = null;
        _client.WithBearer(studentAuth);
        var assignedResp = await _client.GetFromJsonAsync<TrainingPlansSearchPagedResponse>(
            $"/api/training-plans/assigned/student/{studentId}?pageSize=50");
        assignedResp.Should().NotBeNull();
        assignedResp!.Items.Should().Contain(i => i.Id == planId);
    }

    [Fact]
    public async Task CreatePlan_Unauthenticated_ReturnsUnauthorized()
    {
        var req = new CreateTrainingPlanRequest(
            "Sem auth", "d", "i", "n", TrainingSplitType.FullBody, Visibility.Private);
        var resp = await _client.PostAsJsonAsync("/api/training-plans", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

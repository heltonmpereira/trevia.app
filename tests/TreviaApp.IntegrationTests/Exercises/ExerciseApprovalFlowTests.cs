namespace TreviaApp.IntegrationTests.Exercises;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.IntegrationTests.Utilities;
using TreviaApp.Shared.Enums;
using Xunit;

[Collection("Auth Integration Tests")]
public class ExerciseApprovalFlowTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    public ExerciseApprovalFlowTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<AuthResponse> RegisterAsTrainer(string suffix)
    {
        var email = $"trainer_{suffix}_{Guid.NewGuid():N}[..6]@test.com"
            .Replace("[..6]", Guid.NewGuid().ToString("N")[..6]);
        var auth = await _client.RegisterNewStudentAsync(email);
        await IdentityHelpers.ConfirmEmailAsync(_factory, email);
        var loginAuth = await _client.LoginAsync(email, "Student123!");
        return loginAuth;
    }

    private async Task<AuthResponse> LoginAsAdmin()
    {
        return await _client.LoginAsync("admin-integration@test.com", "AdminTest123!");
    }

    [Fact]
    public async Task Trainer_Creates_Submits_Admin_Approves_SearchApproved_Finds()
    {
        var trainerAuth = await RegisterAsTrainer("ex1");
        _client.WithBearer(trainerAuth);

        var createReq = new CreateExerciseRequest(
            Name: "Supino Reto com Barra",
            Environment: TrainingEnvironment.Gym,
            Modality: ExerciseModality.WeightTraining,
            DifficultyLevel: DifficultyLevel.Intermediate,
            MeasurementType: MeasurementType.LoadAndRepetitions,
            Instructions: "Deite no banco, pegue a barra, desça até o peito e empurre para cima.",
            ShortDescription: "Exercício composto para peitoral",
            Tips: "Mantenha os pés firmes no chão",
            Visibility: Visibility.Public,
            Muscles: new[]
            {
                new MuscleMappingRequest(Muscle.Chest, MuscleRole.Primary, 80),
                new MuscleMappingRequest(Muscle.Triceps, MuscleRole.Secondary, 40)
            },
            Equipments: new[]
            {
                new EquipmentMappingRequest(Equipment.Barbell, Required: true),
                new EquipmentMappingRequest(Equipment.Bench, Required: true)
            });

        var createResp = await _client.PostAsJsonAsync("/api/exercises", createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<ExerciseDetailResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Status.Should().BeOneOf(ExerciseStatus.Draft, ExerciseStatus.AwaitingApproval);

        var exId = created.Id;
        var submitResp = await _client.PostAsync($"/api/exercises/{exId}/submit", null);
        submitResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.NoContent);

        _client.DefaultRequestHeaders.Authorization = null;
        var adminAuth = await LoginAsAdmin();
        _client.WithBearer(adminAuth);

        var approveResp = await _client.PutAsync($"/api/exercises/{exId}/approve", null);
        approveResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        _client.DefaultRequestHeaders.Authorization = null;
        var studentAuth = await _client.RegisterNewStudentAsync(
            "student_seek_" + Guid.NewGuid().ToString("N")[..6] + "@test.com");
        _client.WithBearer(studentAuth);

        var approvedResp = await _client.GetAsync("/api/exercises/approved?pageSize=50");
        approvedResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await approvedResp.Content.ReadFromJsonAsync<ExerciseSearchPagedResponse>();
        approved.Should().NotBeNull();
        approved!.Items.Should().Contain(i => i.Id == exId);
    }

    [Fact]
    public async Task CreateExercise_WithoutAuth_ReturnsUnauthorized()
    {
        var req = new CreateExerciseRequest(
            Name: "Exercicio Sem Auth",
            Environment: TrainingEnvironment.Home,
            Modality: ExerciseModality.WeightTraining,
            DifficultyLevel: DifficultyLevel.Beginner,
            MeasurementType: MeasurementType.Bodyweight,
            Instructions: "Instruções");

        var resp = await _client.PostAsJsonAsync("/api/exercises", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveExercise_ByNonAdmin_ReturnsForbiddenOrUnauthorized()
    {
        var trainer = await RegisterAsTrainer("nonadmin");
        _client.WithBearer(trainer);

        var createReq = new CreateExerciseRequest(
            "Agachamento Livre",
            TrainingEnvironment.Gym,
            ExerciseModality.WeightTraining,
            DifficultyLevel.Intermediate,
            MeasurementType.LoadAndRepetitions,
            "Agache até a coxa ficar paralela");

        var created = await _client.PostAsJsonAsync("/api/exercises", createReq);
        var data = await created.Content.ReadFromJsonAsync<ExerciseDetailResponse>();
        var exId = data!.Id;
        await _client.PostAsync($"/api/exercises/{exId}/submit", null);

        var approveResp = await _client.PutAsync($"/api/exercises/{exId}/approve", null);
        approveResp.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed);
    }
}

namespace TreviaApp.IntegrationTests.WorkoutExecution;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Contracts.WorkoutExecution.Responses;
using TreviaApp.Domain.Coaching;
using TreviaApp.Domain.Identity;
using TreviaApp.Domain.WorkoutExecution;
using TreviaApp.Infrastructure.Persistence;
using TreviaApp.IntegrationTests.Utilities;
using TreviaApp.Shared.Constants;
using TreviaApp.Shared.Enums;
using Xunit;

[Collection("Auth Integration Tests")]
public class StudentWorkoutHistoryAccessTests : IAsyncLifetime
{
    private const string DefaultPassword = "Student123!";

    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public StudentWorkoutHistoryAccessTests(TestWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LinkedTrainer_WithWorkoutHistoryPermission_CanViewStudentWorkoutDetail()
    {
        var seeded = await SeedCoachStudentWorkoutAsync(CoachPermissions.CanViewWorkoutHistory);
        var coachAuth = await LoginAsync(seeded.CoachEmail);
        using var authClient = _factory.CreateClient().WithBearer(coachAuth);

        var response = await authClient.GetAsync($"/api/workouts/students/{seeded.StudentId}/sessions/{seeded.WorkoutSessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<WorkoutSessionDetailResponse>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(seeded.WorkoutSessionId);
        payload.StudentId.Should().Be(seeded.StudentId);
        payload.StudentDisplayName.Should().Be("Student Viewer");
        payload.OverallRating.Should().Be(WorkoutRating.Moderate);
    }

    [Fact]
    public async Task LinkedTrainer_WithoutWorkoutHistoryPermission_GetsForbidden()
    {
        var seeded = await SeedCoachStudentWorkoutAsync(CoachPermissions.CanAssignTrainingPlans);
        var coachAuth = await LoginAsync(seeded.CoachEmail);
        using var authClient = _factory.CreateClient().WithBearer(coachAuth);

        var response = await authClient.GetAsync($"/api/workouts/students/{seeded.StudentId}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<AuthResponse> LoginAsync(string email)
        => await _client.LoginAsync(email, DefaultPassword);

    private async Task<(string CoachEmail, Guid StudentId, Guid WorkoutSessionId)> SeedCoachStudentWorkoutAsync(CoachPermissions permissions)
    {
        var coachEmail = $"coach_{Guid.NewGuid():N}@test.com";
        var studentEmail = $"student_{Guid.NewGuid():N}@test.com";

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var coach = new AppUser
        {
            UserName = coachEmail,
            Email = coachEmail,
            FirstName = "Coach",
            LastName = "Viewer",
            DisplayName = "Coach Viewer",
            EmailConfirmed = true
        };

        var student = new AppUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            FirstName = "Student",
            LastName = "Viewer",
            DisplayName = "Student Viewer",
            EmailConfirmed = true
        };

        var coachCreateResult = await userManager.CreateAsync(coach, DefaultPassword);
        coachCreateResult.Succeeded.Should().BeTrue();
        var studentCreateResult = await userManager.CreateAsync(student, DefaultPassword);
        studentCreateResult.Succeeded.Should().BeTrue();

        (await userManager.AddToRoleAsync(coach, AppRoles.Trainer)).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(student, AppRoles.Student)).Succeeded.Should().BeTrue();

        db.Set<CoachStudentLink>().Add(new CoachStudentLink(coach.Id, student.Id, permissions));

        var workout = new WorkoutSession(student.Id, null, null, "Treino A");
        workout.Start();
        workout.Finish(WorkoutRating.Moderate, "Sessao concluida", 180);
        db.Set<WorkoutSession>().Add(workout);

        await db.SaveChangesAsync();

        return (coachEmail, student.Id, workout.Id);
    }
}

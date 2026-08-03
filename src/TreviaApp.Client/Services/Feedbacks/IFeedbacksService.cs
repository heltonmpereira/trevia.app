using TreviaApp.Contracts.Common;
using TreviaApp.Contracts.Feedbacks.Requests;
using TreviaApp.Contracts.Feedbacks.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services.Feedbacks;

public interface IFeedbacksService
{
    Task<WorkoutFeedbackResponse> CreateWorkoutFeedback(Guid sessionId, CreateWorkoutFeedbackRequest request, CancellationToken ct = default);
    Task<ExerciseFeedbackResponse> CreateExerciseFeedback(Guid exerciseId, CreateExerciseFeedbackRequest request, CancellationToken ct = default);
    Task<SetFeedbackResponse> CreateSetFeedback(Guid setId, CreateSetFeedbackRequest request, CancellationToken ct = default);
    Task<UnifiedFeedbackItemResponse> UpdateFeedback(Guid feedbackId, FeedbackLevel level, UpdateFeedbackRequest request, CancellationToken ct = default);
    Task DeleteFeedback(Guid feedbackId, FeedbackLevel level, CancellationToken ct = default);
    Task<FeedbacksBySessionBundleResponse> GetFeedbacksBySession(Guid sessionId, CancellationToken ct = default);
    Task MarkFeedbackAsRead(Guid feedbackId, FeedbackLevel level, CancellationToken ct = default);
    Task<PaginatedResponse<UnifiedFeedbackItemResponse>> GetMyFeedbacks(int page = 1, int pageSize = 20, Guid? workoutSessionId = null, bool? onlyUnread = null, FeedbackLevel? level = null, CancellationToken ct = default);
    Task<PaginatedResponse<UnifiedFeedbackItemResponse>> GetStudentFeedbacks(Guid studentId, int page = 1, int pageSize = 20, Guid? workoutSessionId = null, FeedbackLevel? level = null, CancellationToken ct = default);
    Task<ExerciseFeedbackResponse> RespondToExerciseFeedback(Guid feedbackId, RespondToExerciseFeedbackRequest request, CancellationToken ct = default);
}

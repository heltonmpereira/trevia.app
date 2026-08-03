using TreviaApp.Contracts.Exercises.Requests;
using TreviaApp.Contracts.Exercises.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public interface IExerciseService
{
    Task<ExerciseDetailResponse> Create(CreateExerciseRequest request, CancellationToken ct = default);
    Task<ExerciseDetailResponse> Update(Guid exerciseId, UpdateExerciseRequest request, CancellationToken ct = default);
    Task Delete(Guid exerciseId, CancellationToken ct = default);
    Task SubmitForApproval(Guid exerciseId, CancellationToken ct = default);
    Task Approve(Guid exerciseId, CancellationToken ct = default);
    Task Reject(Guid exerciseId, RejectExerciseRequest request, CancellationToken ct = default);
    Task<ExerciseDetailResponse> GetById(Guid exerciseId, CancellationToken ct = default);
    Task<ExerciseSearchPagedResponse> GetMine(int page = 1, int pageSize = 20, ExerciseStatus? status = null, CancellationToken ct = default);
    Task<ExerciseSearchPagedResponse> SearchApproved(SearchExercisesRequest request, CancellationToken ct = default);
    Task<ExerciseSearchPagedResponse> SearchAll(SearchExercisesRequest filters, bool includeDeleted = false, CancellationToken ct = default);
    Task<int> GetAwaitingApprovalCount(CancellationToken ct = default);

    Task<ExerciseMediaResponse> AddMedia(
        Guid exerciseId,
        Stream stream,
        string fileName,
        string contentType,
        int order = 0,
        string? caption = null,
        bool isPrimary = false,
        MediaType? mediaType = null,
        CancellationToken ct = default);

    Task RemoveMedia(Guid exerciseId, Guid mediaId, CancellationToken ct = default);
    Task SetPrimaryMedia(Guid exerciseId, Guid mediaId, CancellationToken ct = default);

    Task<ExerciseMuscleResponse> AddMuscle(Guid exerciseId, AddMuscleToExerciseRequest dto, CancellationToken ct = default);
    Task RemoveMuscle(Guid exerciseId, int muscle, CancellationToken ct = default);

    Task<ExerciseEquipmentResponse> AddEquipment(Guid exerciseId, AddEquipmentToExerciseRequest dto, CancellationToken ct = default);
    Task RemoveEquipment(Guid exerciseId, int equipment, CancellationToken ct = default);
}

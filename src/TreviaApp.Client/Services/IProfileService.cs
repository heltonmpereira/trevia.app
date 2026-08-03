using TreviaApp.Contracts.Profiles.Requests;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Shared.Enums;

namespace TreviaApp.Client.Services;

public interface IProfileService
{
    Task<ProfileFullResponse> CreateProfile(CreateProfileRequest request, CancellationToken ct = default);
    Task<ProfileFullResponse> UpdateProfile(UpdateProfileRequest request, CancellationToken ct = default);
    Task<ProfileFullResponse> GetMyProfile(CancellationToken ct = default);
    Task<ProfileFullResponse> GetProfileByUserId(Guid userId, CancellationToken ct = default);
    Task DeleteProfile(CancellationToken ct = default);

    Task<WeightEntryResponse> UpsertWeightEntry(UpsertWeightEntryRequest request, CancellationToken ct = default);
    Task<WeightHistoryResponse> GetWeightHistory(int page = 1, int pageSize = 30, CancellationToken ct = default);
    Task DeleteWeightEntry(Guid id, CancellationToken ct = default);

    Task<MeasurementResponse> UpsertMeasurement(UpsertMeasurementRequest request, CancellationToken ct = default);
    Task<MeasurementHistoryResponse> GetMeasurementHistory(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task DeleteMeasurement(Guid id, CancellationToken ct = default);

    Task<PhotoUploadResultResponse> UploadProfilePhoto(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task RemoveProfilePhoto(CancellationToken ct = default);

    Task<List<Equipment>> UpdateEquipments(UpdateEquipmentsRequest request, CancellationToken ct = default);
}

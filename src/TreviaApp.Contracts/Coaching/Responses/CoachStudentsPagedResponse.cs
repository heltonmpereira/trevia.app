using TreviaApp.Contracts.Common;

namespace TreviaApp.Contracts.Coaching.Responses;

/// <summary>
/// Response payload for CoachStudentsPagedResponse.
/// </summary>
public class CoachStudentsPagedResponse : PaginatedResponse<CoachStudentSummaryResponse>
{
}

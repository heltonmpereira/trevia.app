namespace TreviaApp.Application.Profiles.Commands.CreateProfile;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TreviaApp.Application.Abstractions.Data;
using TreviaApp.Application.Abstractions.Messaging;
using TreviaApp.Application.Profiles.Mappings;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Profiles.Responses;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Profiles;

public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, ProfileFullResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateProfileCommandHandler> _logger;

    public CreateProfileCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CreateProfileCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ProfileFullResponse> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new DomainException(
            "Usuário não autenticado.",
            nameof(ErrorCodes.Unauthorized));

        var existing = await _db.Set<UserProfile>()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId, cancellationToken);

        if (existing)
            throw new DomainException(
                "Perfil já existe para este usuário.",
                "ProfileAlreadyExists");

        var profile = new UserProfile(
            userId,
            request.Goal,
            request.Experience,
            request.PreferredEnvironment,
            request.PrivacyLevel,
            request.PreferredUnits,
            request.Bio);

        _db.Set<UserProfile>().Add(profile);

        _logger.LogInformation(
            "ProfileCreated ProfileId={ProfileId} UserId={UserId}",
            profile.Id,
            userId);

        return ProfileMappings.MapToFullResponse(profile);
    }
}

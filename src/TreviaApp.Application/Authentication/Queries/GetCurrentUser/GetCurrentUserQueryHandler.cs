namespace TreviaApp.Application.Authentication.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TreviaApp.Application.Security;
using TreviaApp.Contracts.Authentication;
using TreviaApp.Domain.Exceptions;
using TreviaApp.Domain.Identity;
using TreviaApp.Shared.Constants;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly ICurrentUserService _current;
    private readonly UserManager<AppUser> _userManager;

    public GetCurrentUserQueryHandler(ICurrentUserService current, UserManager<AppUser> userManager)
    {
        _current = current;
        _userManager = userManager;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_current.UserId.HasValue)
            throw new DomainException("Não autenticado.", ErrorCodes.Unauthorized);

        var user = await _userManager.FindByIdAsync(_current.UserId.Value.ToString());
        if (user is null)
            throw new DomainException("Usuário não encontrado.", ErrorCodes.NotFound);

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        return new CurrentUserResponse(user.Id, user.Email!, user.EmailConfirmed, user.FirstName, user.LastName, user.DisplayName, user.CreatedAt, user.LastActiveAt, roles);
    }
}

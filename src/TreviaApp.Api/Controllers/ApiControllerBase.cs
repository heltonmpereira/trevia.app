namespace TreviaApp.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TreviaApp.Application.Security;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;
    private ICurrentUserService? _currentUser;
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    protected ICurrentUserService CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
}

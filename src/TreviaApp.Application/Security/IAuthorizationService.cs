namespace TreviaApp.Application.Security;

public interface IAuthorizationService
{
    Task AuthorizeAsync(object user, object resource, string policy);
}

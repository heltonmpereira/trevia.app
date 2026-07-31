namespace TreviaApp.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TreviaApp.Application.Consents.Commands.GiveConsentBatch;
using TreviaApp.Application.Consents.Commands.RevokeConsent;
using TreviaApp.Application.Consents.Queries.GetConsentVersions;
using TreviaApp.Application.Consents.Queries.GetMyConsents;
using TreviaApp.Contracts.Consents.Requests;
using TreviaApp.Contracts.Consents.Responses;

[ApiController]
[Route("api/consents")]
[Produces("application/json")]
[EnableRateLimiting("AuthEndpoint")]
public class ConsentsController : ApiControllerBase
{
    [HttpPost("give")]
    [Authorize]
    [ProducesResponseType(typeof(List<ConsentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GiveConsentBatch([FromBody] GiveConsentBatchRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new GiveConsentBatchCommand(request.Consents), ct);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeConsent([FromBody] RevokeConsentRequest request, CancellationToken ct)
    {
        await Sender.Send(new RevokeConsentCommand(request.ConsentType, request.Reason), ct);
        return NoContent();
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(List<ConsentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyConsents(
        [FromQuery] bool includeRevoked = true,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyConsentsQuery(includeRevoked), ct);
        return Ok(result);
    }

    [HttpGet("versions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ConsentVersionInfoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsentVersions(CancellationToken ct)
    {
        var result = await Sender.Send(new GetConsentVersionsQuery(), ct);
        return Ok(result);
    }
}

using AccountService.Application.ProfileAscents.RebuildProfileStats;
using Common.API.Results;
using Common.API.Security;
using Common.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/admin/profiles")]
[Authorize(Policy = AuthorizationExtensions.AdminPolicyName)]
public sealed class AdminProfilesController(ISender sender) : ControllerBase
{
    [HttpPost("{userId:guid}/stats/rebuild")]
    [ProducesResponseType(typeof(ProfileStatsRebuildResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RebuildStats(Guid userId, CancellationToken cancellationToken)
    {
        Result<ProfileStatsRebuildResponse> result =
            await sender.Send(new RebuildProfileStatsCommand(userId), cancellationToken);

        return result.ToActionResult();
    }
}

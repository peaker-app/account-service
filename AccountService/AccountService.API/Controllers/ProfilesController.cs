using AccountService.API.Requests;
using AccountService.Application.Abstractions;
using AccountService.Application.Profiles.GetMyProfile;
using AccountService.Application.Profiles.GetPublicProfile;
using AccountService.Application.Profiles.RemoveAvatar;
using AccountService.Application.Profiles.UploadAvatar;
using Common.API.Results;
using Common.Application.Abstractions;
using Common.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController(ISender sender, IUserContext userContext) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        Result<ProfileResponse> result = await sender.Send(
            new GetMyProfileQuery(userContext.UserId), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(request.ToCommand(userContext.UserId), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("me/slug")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeSlug(ChangeSlugRequest request, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(request.ToCommand(userContext.UserId), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("me/avatar")]
    [Authorize]
    [ProducesResponseType(typeof(AvatarResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest();
        }

        AvatarUpload upload = await ReadUploadAsync(file, cancellationToken);
        Result<AvatarResponse> result = await sender.Send(
            new UploadAvatarCommand(userContext.UserId, upload), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("me/avatar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAvatar(CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new RemoveAvatarCommand(userContext.UserId), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicProfile(Guid userId, CancellationToken cancellationToken)
    {
        Result<PublicProfileResponse> result = await sender.Send(
            new GetPublicProfileByIdQuery(userId, CurrentUserOrNull), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicProfileBySlug(string slug, CancellationToken cancellationToken)
    {
        Result<PublicProfileResponse> result = await sender.Send(
            new GetPublicProfileBySlugQuery(slug, CurrentUserOrNull), cancellationToken);

        return result.ToActionResult();
    }

    private Guid? CurrentUserOrNull => userContext.IsAuthenticated ? userContext.UserId : null;

    private static async Task<AvatarUpload> ReadUploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        return new AvatarUpload(stream.ToArray(), file.ContentType, file.FileName);
    }
}

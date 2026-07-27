using AccountService.API.Requests;
using AccountService.Application.Collections.DeleteCollection;
using AccountService.Application.Collections.GetCollectionById;
using AccountService.Application.Collections.ListCollections;
using AccountService.Application.Collections.RemoveCollectionPeak;
using Common.API.Responses;
using Common.API.Results;
using Common.Application.Abstractions;
using Common.Application.Pagination;
using Common.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public sealed class CollectionsController(ISender sender, IUserContext userContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCollectionRequest request, CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(request.ToCommand(userContext.UserId), cancellationToken);

        return result.ToActionResult(id => CreatedAtAction(nameof(GetById), new { id }, id));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CollectionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListMine(
        [FromQuery] ListCollectionsRequest request,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<CollectionSummaryResponse>> result =
            await sender.Send(request.ToQuery(userContext.UserId), cancellationToken);

        return result.ToActionResult(paged => Ok(paged.ToPagedResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] ListCollectionsRequest request,
        CancellationToken cancellationToken)
    {
        Result<CollectionDetailResponse> result =
            await sender.Send(request.ToQuery(userContext.UserId, id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCollectionRequest request,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(request.ToCommand(userContext.UserId, id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new DeleteCollectionCommand(userContext.UserId, id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/peaks")]
    [ProducesResponseType(typeof(CollectionPeakResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddPeak(
        Guid id,
        AddCollectionPeakRequest request,
        CancellationToken cancellationToken)
    {
        Result<CollectionPeakResponse> result =
            await sender.Send(request.ToCommand(userContext.UserId, id), cancellationToken);

        return result.ToActionResult(peak => CreatedAtAction(nameof(GetById), new { id }, peak));
    }

    [HttpDelete("{id:guid}/peaks/{peakId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePeak(Guid id, Guid peakId, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(
            new RemoveCollectionPeakCommand(userContext.UserId, id, peakId), cancellationToken);

        return result.ToActionResult();
    }
}

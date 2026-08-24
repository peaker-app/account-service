using AccountService.Domain.ProfileAscents;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

internal sealed class ProfileAscentRepository(AccountDbContext context) : IProfileAscentRepository
{
    public Task<ProfileAscent?> GetByAscentIdAsync(Guid ascentId, CancellationToken cancellationToken) =>
        context.ProfileAscents.FirstOrDefaultAsync(ascent => ascent.AscentId == ascentId, cancellationToken);

    public async Task<IReadOnlyCollection<ProfileAscent>> GetByProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken) =>
        await context.ProfileAscents
            .Where(ascent => ascent.ProfileId == profileId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ProfileAscent>> GetByPeakAsync(
        Guid peakId,
        CancellationToken cancellationToken) =>
        await context.ProfileAscents
            .Where(ascent => ascent.Peak.PeakId == peakId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByAscentIdAsync(Guid ascentId, CancellationToken cancellationToken) =>
        context.ProfileAscents.AnyAsync(ascent => ascent.AscentId == ascentId, cancellationToken);

    public void Add(ProfileAscent ascent) => context.ProfileAscents.Add(ascent);

    public void Remove(ProfileAscent ascent) => context.ProfileAscents.Remove(ascent);
}

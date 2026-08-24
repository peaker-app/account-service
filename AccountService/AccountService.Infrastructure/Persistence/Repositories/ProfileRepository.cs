using AccountService.Domain.Profiles;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

internal sealed class ProfileRepository(AccountDbContext context) : IProfileRepository
{
    public Task<Profile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken) =>
        context.Profiles.FirstOrDefaultAsync(profile => profile.Id == profileId, cancellationToken);

    public Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public Task<Profile?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        Result<ProfileSlug> parsed = ProfileSlug.Create(slug);

        return parsed.IsFailure
            ? Task.FromResult<Profile?>(null)
            : context.Profiles.FirstOrDefaultAsync(profile => profile.Slug == parsed.Value, cancellationToken);
    }

    public async Task<Guid?> FindIdByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await context.Profiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Profiles.AnyAsync(profile => profile.UserId == userId, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        Result<ProfileSlug> parsed = ProfileSlug.Create(slug);

        return parsed.IsFailure
            ? Task.FromResult(false)
            : context.Profiles.AnyAsync(profile => profile.Slug == parsed.Value, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetKnownAvatarPublicIdsAsync(
        IReadOnlyCollection<string> candidates,
        CancellationToken cancellationToken)
    {
        List<string> known = await context.Profiles
            .Where(profile => profile.Avatar != null && candidates.Contains(profile.Avatar.PublicId))
            .Select(profile => profile.Avatar!.PublicId)
            .ToListAsync(cancellationToken);

        return known.ToHashSet(StringComparer.Ordinal);
    }

    public void Add(Profile profile) => context.Profiles.Add(profile);

    public void Remove(Profile profile) => context.Profiles.Remove(profile);
}

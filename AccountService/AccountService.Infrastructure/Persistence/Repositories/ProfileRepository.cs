using AccountService.Domain.Profiles;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Persistence.Repositories;

internal sealed class ProfileRepository(AccountDbContext context) : IProfileRepository
{
    public Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public Task<Profile?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        Result<ProfileSlug> parsed = ProfileSlug.Create(slug);

        return parsed.IsFailure
            ? Task.FromResult<Profile?>(null)
            : context.Profiles.FirstOrDefaultAsync(profile => profile.Slug == parsed.Value, cancellationToken);
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Profiles.AnyAsync(profile => profile.UserId == userId, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        Result<ProfileSlug> parsed = ProfileSlug.Create(slug);

        return parsed.IsFailure
            ? Task.FromResult(false)
            : context.Profiles.AnyAsync(profile => profile.Slug == parsed.Value, cancellationToken);
    }

    public void Add(Profile profile) => context.Profiles.Add(profile);
}

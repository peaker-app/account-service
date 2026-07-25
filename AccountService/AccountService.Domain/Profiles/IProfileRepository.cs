namespace AccountService.Domain.Profiles;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken);

    Task<Profile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Profile?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);

    void Add(Profile profile);

    void Remove(Profile profile);
}

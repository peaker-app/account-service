namespace AccountService.Domain.ProfileAscents;

public interface IProfileAscentRepository
{
    Task<ProfileAscent?> GetByAscentIdAsync(Guid ascentId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProfileAscent>> GetByProfileAsync(Guid profileId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProfileAscent>> GetByPeakAsync(Guid peakId, CancellationToken cancellationToken);

    Task<bool> ExistsByAscentIdAsync(Guid ascentId, CancellationToken cancellationToken);

    void Add(ProfileAscent ascent);

    void Remove(ProfileAscent ascent);
}

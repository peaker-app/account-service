using AccountService.Domain.Profiles;
using Common.Application.Abstractions;
using Common.Application.Messaging;
using Common.Domain.Results;

namespace AccountService.Application.Profiles.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        Result<ProfileDetails> details = BuildDetails(command);
        if (details.IsFailure)
        {
            return Result.Failure(details.Error);
        }

        Profile? profile = await profileRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(ProfileErrors.NotFound(command.UserId));
        }

        Result updated = profile.UpdateDetails(details.Value);
        if (updated.IsFailure)
        {
            return updated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Result<ProfileDetails> BuildDetails(UpdateProfileCommand command)
    {
        Result<DisplayName> displayName = DisplayName.Create(command.DisplayName);
        if (displayName.IsFailure)
        {
            return Result.Failure<ProfileDetails>(displayName.Error);
        }

        CountryCode? countryCode = null;
        if (!string.IsNullOrWhiteSpace(command.CountryCode))
        {
            Result<CountryCode> parsed = CountryCode.Create(command.CountryCode);
            if (parsed.IsFailure)
            {
                return Result.Failure<ProfileDetails>(parsed.Error);
            }

            countryCode = parsed.Value;
        }

        return new ProfileDetails(displayName.Value, command.Bio, countryCode, command.Visibility);
    }
}

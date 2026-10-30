using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;

namespace KidsPartyVenue.Application.Users.ChangePreferredLanguage;

internal sealed class ChangePreferredLanguageCommandHandler(
    IUserContext userContext,
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangePreferredLanguageCommand>
{
    public async Task<Result> Handle(ChangePreferredLanguageCommand command, CancellationToken cancellationToken)
    {
        UserProfile? profile = await userProfileRepository.GetByIdAsync(userContext.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        Result result = profile.ChangePreferredLanguage(command.Language);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

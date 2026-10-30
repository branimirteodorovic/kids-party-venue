using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Abstractions.Localization;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;

namespace KidsPartyVenue.Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    ILanguageContext languageContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.CreateVersion7();

        Result<UserProfile> profileResult = UserProfile.Create(
            userId,
            command.FullName,
            command.PreferredLanguage ?? languageContext.Current,
            dateTimeProvider.UtcNow);

        if (profileResult.IsFailure)
        {
            return Result.Failure<Guid>(profileResult.Error);
        }

        // The account, its role and the profile are created together or not at all
        await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Result registerResult = await identityService.RegisterAsync(userId, command.Email, command.Password, cancellationToken);
        if (registerResult.IsFailure)
        {
            return Result.Failure<Guid>(registerResult.Error);
        }

        userProfileRepository.Insert(profileResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return userId;
    }
}

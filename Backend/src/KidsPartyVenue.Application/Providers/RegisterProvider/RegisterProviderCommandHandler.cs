using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.RegisterProvider;

internal sealed class RegisterProviderCommandHandler(
    IUserContext userContext,
    IProviderRepository providerRepository,
    IProviderPlaceLookup placeLookup,
    IProviderSlugGenerator slugGenerator,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RegisterProviderCommand, RegisterProviderResponse>
{
    public async Task<Result<RegisterProviderResponse>> Handle(RegisterProviderCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Result<ProviderPlace> placeResult = await placeLookup.FindAsync(command.CityId, command.DistrictId, cancellationToken);
        if (placeResult.IsFailure)
        {
            return Result.Failure<RegisterProviderResponse>(placeResult.Error);
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            Result<PhoneNumber> phoneResult = PhoneNumber.Create(command.Phone);
            if (phoneResult.IsFailure)
            {
                return Result.Failure<RegisterProviderResponse>(phoneResult.Error);
            }

            phone = phoneResult.Value;
        }

        Result<GeoCoordinate> locationResult = GeoCoordinate.Create(command.Latitude, command.Longitude);
        if (locationResult.IsFailure)
        {
            return Result.Failure<RegisterProviderResponse>(locationResult.Error);
        }

        string slug = await slugGenerator.GenerateAsync(command.Name, cancellationToken);
        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<Provider> providerResult = Provider.Create(
            slug,
            command.Name,
            command.Category,
            command.Description,
            command.Address,
            command.CityId,
            placeResult.Value.District,
            placeResult.Value.CityHasDistricts,
            locationResult.Value,
            phone,
            isClaimed: true,
            utcNow);

        if (providerResult.IsFailure)
        {
            return Result.Failure<RegisterProviderResponse>(providerResult.Error);
        }

        Provider provider = providerResult.Value;

        Result memberResult = provider.AddMember(userId, ProviderMemberRole.Owner, utcNow);
        if (memberResult.IsFailure)
        {
            return Result.Failure<RegisterProviderResponse>(memberResult.Error);
        }

        // The role, the provider and its Owner are saved together or not at all. The role goes first because the
        // identity service saves on its own.
        await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Result roleResult = await identityService.EnsureProviderMemberRoleAsync(userId, cancellationToken);
        if (roleResult.IsFailure)
        {
            return Result.Failure<RegisterProviderResponse>(roleResult.Error);
        }

        providerRepository.Insert(provider);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // The cached memberships and permissions don't know the new provider and role yet
        await cacheService.RemoveByTagAsync(ProviderMembershipCache.UserTag(userId), cancellationToken);

        return new RegisterProviderResponse(provider.Id, provider.Slug);
    }
}

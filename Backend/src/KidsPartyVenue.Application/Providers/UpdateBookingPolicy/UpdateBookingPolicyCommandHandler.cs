using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UpdateBookingPolicy;

internal sealed class UpdateBookingPolicyCommandHandler(
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cacheService) : ICommandHandler<UpdateBookingPolicyCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(UpdateBookingPolicyCommand command, CancellationToken cancellationToken)
    {
        Provider? provider = await providerRepository.GetByIdAsync(command.ProviderId, cancellationToken);
        if (provider is null)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.NotFound);
        }

        if (providerRepository.GetVersion(provider) != command.Version)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.ConcurrencyConflict);
        }

        Result<BookingPolicy> policyResult = BookingPolicy.Create(
            command.DepositType,
            command.DepositValue,
            command.DepositDueDays,
            command.FreeCancellationDays);

        if (policyResult.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(policyResult.Error);
        }

        provider.ChangeBookingPolicy(policyResult.Value, dateTimeProvider.UtcNow);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.ConcurrencyConflict);
        }

        await cacheService.RemoveByTagAsync(ProviderCache.Tag(provider.Id), cancellationToken);

        return new ProviderVersionResponse(providerRepository.GetVersion(provider));
    }
}

using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UnsuspendProvider;

internal sealed class UnsuspendProviderCommandHandler(
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cacheService) : ICommandHandler<UnsuspendProviderCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(UnsuspendProviderCommand command, CancellationToken cancellationToken)
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

        Result result = provider.Unsuspend(dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(result.Error);
        }

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

using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.RequestProviderChanges;

internal sealed class RequestProviderChangesCommandHandler(
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RequestProviderChangesCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(RequestProviderChangesCommand command, CancellationToken cancellationToken)
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

        Result result = provider.RequestChanges(command.Notes, dateTimeProvider.UtcNow);
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

        // Not public before or after, so no cached page to remove
        return new ProviderVersionResponse(providerRepository.GetVersion(provider));
    }
}

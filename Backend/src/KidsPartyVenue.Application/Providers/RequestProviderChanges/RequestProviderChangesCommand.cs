using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.RequestProviderChanges;

/// <summary>Sends a profile waiting for review back to its owner with notes.</summary>
public sealed record RequestProviderChangesCommand(Guid ProviderId, string Notes, uint Version) : ICommand<ProviderVersionResponse>;

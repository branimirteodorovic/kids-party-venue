using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.ApproveProvider;

/// <summary>Publishes a profile waiting for review.</summary>
public sealed record ApproveProviderCommand(Guid ProviderId, uint Version) : ICommand<ProviderVersionResponse>;

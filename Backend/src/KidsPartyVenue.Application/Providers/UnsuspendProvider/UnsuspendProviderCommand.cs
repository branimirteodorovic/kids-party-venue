using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.UnsuspendProvider;

/// <summary>Lifts a suspension, so the profile is visible again if it is published.</summary>
public sealed record UnsuspendProviderCommand(Guid ProviderId, uint Version) : ICommand<ProviderVersionResponse>;

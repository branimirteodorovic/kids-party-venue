using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.SuspendProvider;

/// <summary>Hides a profile from parents, keeping its review status.</summary>
public sealed record SuspendProviderCommand(Guid ProviderId, string Reason, uint Version) : ICommand<ProviderVersionResponse>;

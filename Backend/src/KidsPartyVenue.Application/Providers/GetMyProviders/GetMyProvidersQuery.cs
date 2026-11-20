using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.GetMyProviders;

/// <summary>The providers the current user is a member of, for the provider panel.</summary>
public sealed record GetMyProvidersQuery : IQuery<List<MyProviderResponse>>;

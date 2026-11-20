using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.GetPublicProvider;

/// <summary>The profile parents see, only while the provider is publicly visible.</summary>
public sealed record GetPublicProviderQuery(string Slug) : IQuery<PublicProviderResponse>;

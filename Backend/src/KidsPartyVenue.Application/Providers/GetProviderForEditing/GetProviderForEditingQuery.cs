using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.GetProviderForEditing;

/// <summary>Everything the profile editor shows, in any status, with the version its next change must send.</summary>
public sealed record GetProviderForEditingQuery(Guid ProviderId) : IQuery<ProviderForEditingResponse>;

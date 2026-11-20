using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.SubmitProviderForReview;

/// <summary>Sends a complete draft, or a profile the admin sent back, to review.</summary>
public sealed record SubmitProviderForReviewCommand(Guid ProviderId, uint Version) : ICommand<ProviderVersionResponse>;

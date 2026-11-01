using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

/// <summary>The open cities with their open categories and districts, plus the amenities, in the request language.</summary>
public sealed record GetCatalogsQuery : IQuery<CatalogsResponse>;

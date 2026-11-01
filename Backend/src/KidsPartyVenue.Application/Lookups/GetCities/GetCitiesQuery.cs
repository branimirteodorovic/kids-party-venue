using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Lookups.GetCities;

/// <summary>Every city, open or not, with its districts, in the request language.</summary>
public sealed record GetCitiesQuery : IQuery<List<CityResponse>>;

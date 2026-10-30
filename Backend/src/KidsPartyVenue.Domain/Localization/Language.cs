using System.Text.Json.Serialization;

namespace KidsPartyVenue.Domain.Localization;

// Always serialized as its code ("sr", "en", "ru"), whatever converters the caller configures
[JsonConverter(typeof(JsonStringEnumConverter<Language>))]
public enum Language
{
    [JsonStringEnumMemberName("sr")]
    Sr,

    [JsonStringEnumMemberName("en")]
    En,

    [JsonStringEnumMemberName("ru")]
    Ru
}

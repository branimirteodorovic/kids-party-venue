namespace KidsPartyVenue.Application.Authorization;

/// <summary>Named policies that check more than a permission. Permission policies are named after the permission itself.</summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// An owner of the provider in the route, or a user with <see cref="Permissions.ProvidersManageAny"/>. The route must
    /// name the provider <see cref="ProviderIdRouteValue"/>.
    /// </summary>
    public const string ProviderMember = "ProviderMember";

    public const string ProviderIdRouteValue = "providerId";
}

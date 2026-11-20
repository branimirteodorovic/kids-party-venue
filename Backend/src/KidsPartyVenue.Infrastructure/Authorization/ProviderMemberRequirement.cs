using Microsoft.AspNetCore.Authorization;

namespace KidsPartyVenue.Infrastructure.Authorization;

/// <summary>The user is a member of the provider in the route, or may manage any provider.</summary>
internal sealed class ProviderMemberRequirement : IAuthorizationRequirement;

using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

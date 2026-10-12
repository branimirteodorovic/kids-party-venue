using KidsPartyVenue.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Database;

public class KidsPartyVenueDbContext(DbContextOptions<KidsPartyVenueDbContext> options) : DbContext(options), IUnitOfWork
{
}

namespace KidsPartyVenue.Domain.Venues;

/// <summary>Whether a venue serves food, and whether its price includes it.</summary>
public enum FoodService
{
    /// <summary>The venue serves no food.</summary>
    None,

    /// <summary>The venue sells food that isn't in the price.</summary>
    ForPurchase,

    /// <summary>The price includes food for the children.</summary>
    Included
}

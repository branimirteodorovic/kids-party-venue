namespace KidsPartyVenue.Domain.Venues;

/// <summary>
/// The facts about a venue that parents filter and decide by: capacity, age range, food rules and what the venue
/// offers itself. Prices of food, animators and photographers belong to offerings, not here.
/// </summary>
public sealed record VenueDetails
{
    public const int GuestLimit = 500;
    public const int AgeLimit = 18;

    private VenueDetails(
        int maxChildren,
        int? maxAdults,
        int minAge,
        int maxAge,
        OutsideFoodRule outsideCatering,
        OutsideFoodRule outsideCake,
        FoodService foodService,
        bool hasOwnAnimator,
        bool hasOwnPhotographer,
        bool hasSmokingArea)
    {
        MaxChildren = maxChildren;
        MaxAdults = maxAdults;
        MinAge = minAge;
        MaxAge = maxAge;
        OutsideCatering = outsideCatering;
        OutsideCake = outsideCake;
        FoodService = foodService;
        HasOwnAnimator = hasOwnAnimator;
        HasOwnPhotographer = hasOwnPhotographer;
        HasSmokingArea = hasSmokingArea;
    }

    public int MaxChildren { get; }

    /// <summary>Null when the venue sets no separate limit for adults; 0 when adults don't stay.</summary>
    public int? MaxAdults { get; }

    /// <summary>In whole years, inclusive.</summary>
    public int MinAge { get; }

    /// <summary>In whole years, inclusive; 18 means no upper limit.</summary>
    public int MaxAge { get; }

    public OutsideFoodRule OutsideCatering { get; }

    public OutsideFoodRule OutsideCake { get; }

    public FoodService FoodService { get; }

    public bool HasOwnAnimator { get; }

    public bool HasOwnPhotographer { get; }

    public bool HasSmokingArea { get; }

    public static Result<VenueDetails> Create(
        int maxChildren,
        int? maxAdults,
        int minAge,
        int maxAge,
        OutsideFoodRule outsideCatering,
        OutsideFoodRule outsideCake,
        FoodService foodService,
        bool hasOwnAnimator,
        bool hasOwnPhotographer,
        bool hasSmokingArea)
    {
        if (maxChildren is < 1 or > GuestLimit)
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.InvalidMaxChildren);
        }

        if (maxAdults is < 0 or > GuestLimit)
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.InvalidMaxAdults);
        }

        if (minAge is < 0 or > AgeLimit || maxAge is < 0 or > AgeLimit)
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.InvalidAge);
        }

        if (minAge > maxAge)
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.InvalidAgeRange);
        }

        if (!Enum.IsDefined(outsideCatering) || !Enum.IsDefined(outsideCake))
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.UnknownOutsideFoodRule);
        }

        if (!Enum.IsDefined(foodService))
        {
            return Result.Failure<VenueDetails>(VenueDetailsErrors.UnknownFoodService);
        }

        return new VenueDetails(
            maxChildren,
            maxAdults,
            minAge,
            maxAge,
            outsideCatering,
            outsideCake,
            foodService,
            hasOwnAnimator,
            hasOwnPhotographer,
            hasSmokingArea);
    }
}

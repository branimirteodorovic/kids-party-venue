using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.Domain.Users;

/// <summary>
/// Everything about a user that isn't needed to log in. Shares its ID with the Identity user.
/// </summary>
public sealed class UserProfile : Entity
{
    public const int FullNameMaxLength = 200;

    private UserProfile()
    {
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    /// <summary>The language of messages sent to the user, such as emails and SMS.</summary>
    public Language PreferredLanguage { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Result<UserProfile> Create(Guid userId, string fullName, Language preferredLanguage, DateTime createdAtUtc)
    {
        string trimmedName = fullName?.Trim() ?? string.Empty;

        if (trimmedName.Length == 0)
        {
            return Result.Failure<UserProfile>(UserErrors.FullNameRequired);
        }

        if (trimmedName.Length > FullNameMaxLength)
        {
            return Result.Failure<UserProfile>(UserErrors.FullNameTooLong);
        }

        if (!Enum.IsDefined(preferredLanguage))
        {
            return Result.Failure<UserProfile>(LocalizationErrors.UnsupportedLanguage);
        }

        return new UserProfile
        {
            Id = userId,
            FullName = trimmedName,
            PreferredLanguage = preferredLanguage,
            CreatedAtUtc = createdAtUtc
        };
    }

    public Result ChangePreferredLanguage(Language language)
    {
        if (!Enum.IsDefined(language))
        {
            return Result.Failure(LocalizationErrors.UnsupportedLanguage);
        }

        PreferredLanguage = language;

        return Result.Success();
    }
}

namespace KidsPartyVenue.Application.Authorization;

public static class Permissions
{
    public const string UsersReadSelf = "users:read-self";
    public const string ProvidersCreate = "providers:create";      // admin creates unclaimed provider
    public const string ProvidersRegister = "providers:register";  // logged-in user self-registers
    public const string ProvidersReview = "providers:review";
    public const string ProvidersManageAny = "providers:manage-any";
    public const string BillingManage = "billing:manage";
    public const string AuditRead = "audit:read";
}


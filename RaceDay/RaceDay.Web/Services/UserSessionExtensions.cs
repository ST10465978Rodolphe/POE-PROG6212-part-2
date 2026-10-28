using RaceDay.Contracts;

namespace RaceDay.Web.Services;

// The names under which the MVC site stores things in ITS OWN session
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
    public const string FullName = "FullName";

    // The API's session cookie, kept so every call to the API is made as the logged-in user
    public const string ApiCookie = "ApiCookie";
}

// Small helpers to read and write the logged-in user in the MVC session
public static class UserSessionExtensions
{
    public static bool IsLoggedIn(this ISession session) =>
        session.GetInt32(SessionKeys.UserId) != null;

    public static int GetUserId(this ISession session) =>
        session.GetInt32(SessionKeys.UserId) ?? 0;

    public static string GetRole(this ISession session) =>
        session.GetString(SessionKeys.Role) ?? string.Empty;

    public static string GetFullName(this ISession session) =>
        session.GetString(SessionKeys.FullName) ?? string.Empty;

    // Stores who just logged in. The API cookie is stored separately by ApiClient.
    public static void SignIn(this ISession session, AuthResponse user)
    {
        session.SetInt32(SessionKeys.UserId, user.UserId);
        session.SetString(SessionKeys.Role, user.Role);
        session.SetString(SessionKeys.FullName, $"{user.FirstName} {user.LastName}");
    }
}
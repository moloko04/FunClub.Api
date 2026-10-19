namespace FunClub.Api.Services;

public static class SessionExtensions
{
    public const string UserIdKey = "UserId";
    public const string RoleKey = "Role";
    public const string NameKey = "Name";

    public static void SetUser(this ISession session, int userId, string role, string name)
    {
        session.SetInt32(UserIdKey, userId);
        session.SetString(RoleKey, role);
        session.SetString(NameKey, name);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32(UserIdKey);
    public static string? GetRole(this ISession session) => session.GetString(RoleKey);
    public static string? GetName(this ISession session) => session.GetString(NameKey);
}

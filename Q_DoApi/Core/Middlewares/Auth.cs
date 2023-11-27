namespace OrderApiFun.Core.Middlewares;

public static class Auth
{
    public static string Role_User { get; private set; } = "User";
    public static string Role_Rider { get; private set; } = "Rider";
    public const string UserId = "userId";
    public const string RiderId = "riderId";
    public const string AdminId = "adminId";
    /// <summary>
    /// 仅用在FunctionContext.Items中, 用于标识当前用户的角色
    /// </summary>
    public const string ContextRole = "ContextRole";

    public static void Init(string userRole, string riderRole)
    {
        Role_User = userRole;
        Role_Rider = riderRole;
    }
}
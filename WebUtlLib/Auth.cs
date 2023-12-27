namespace OrderApiFun.Core.Middlewares;

public static class Auth
{
    public const string Role_User = "User";
    public const string Role_Rider = "Rider";
    public const string UserId = "userId";
    public const string RiderId = "riderId";
    public const string AdminId = "adminId";

    /// <summary>
    /// 仅用在FunctionContext.Items中, 用于标识当前用户的角色
    /// </summary>
    public const string ContextRole = "ContextRole";
}
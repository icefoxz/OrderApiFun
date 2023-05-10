using Newtonsoft.Json;
using OrderHelperLib.DtoModels.DeliveryOrders;
using OrderHelperLib.DtoModels.Users;

namespace OrderHelperLib.Utl
{
    public static class Api
    {
        public static ApiCaller Caller { get; private set; }

        public static void Init(string serverUrl) => Caller = new ApiCaller(serverUrl);

        private static void Call(string method, Action<string> onSuccessAction, Action<string> onFailedAction) =>
            Caller.Call(method, onSuccessAction, onFailedAction);

        private static void Call(string method, string content, Action<string> onSuccessAction,
            Action<string> onFailedAction) => Caller.Call(method, content, onSuccessAction, onFailedAction);

        private static void Call(string method, string content, Action<string> onSuccessAction,
            Action<string> onFailedAction, params (string, string)[] queryParams) =>
            Caller.Call(method, content, onSuccessAction, onFailedAction, true, queryParams);

        // 登录方法  // Define constant strings for API methods
        private const string RegisterApi = "Anonymous_RegisterApi";
        private const string LoginApi = "Anonymous_LoginApi";
        private const string ReloginApi = "User_ReloginApi";
        private const string TestApi = "User_TestApi";
        private const string CreateRiderApi = "User_CreateRider";
        private const string CreateDeliveryOrderApi = "User_CreateDeliveryOrder";
        private const string AssignRiderApi = "Rider_AssignRider";
        private const string UpdateOrderStatusApi = "Rider_UpdateOrderStatus";

        // Register
        public static void Register(string username, string email, string password, Action<string> onSuccessAction,
            Action<string> onFailedAction)
        {
            var content = new RegisterDto
            {
                Username = username,
                Email = email,
                Password = password
            };
            Caller.CallWithoutToken(RegisterApi, JsonConvert.SerializeObject(content), onSuccessAction, onFailedAction);
        }

        // Login
        public static void Login(string username, string password, Action<string> onSuccessAction,
            Action<string> onFailedAction)
        {
            var content = new LoginDto
            {
                Username = username,
                Password = password
            };
            Call(LoginApi, JsonConvert.SerializeObject(content), arg =>
            {
                var result = JsonConvert.DeserializeObject<LoginResult>(arg);
                Caller.RegAccessToken(result.access_token);
                onSuccessAction?.Invoke(arg);
            }, onFailedAction);

        }

        private class LoginResult
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }
        private class ReLoginResult
        {
            public string access_token { get; set; }
        }

        // Relogin
        public static void Relogin(string refreshToken, string username, Action<string> onSuccessAction,
            Action<string> onFailedAction)
        {
            var content = new RefreshTokenDto
            {
                Username = username
            };
            Caller.RefreshTokenCall(ReloginApi, refreshToken, JsonConvert.SerializeObject(content), arg =>
            {
                Caller.RegAccessToken(JsonConvert.DeserializeObject<ReLoginResult>(arg).access_token);
                onSuccessAction?.Invoke(arg);
            },
                onFailedAction);
        }

        // TestApi
        public static void Test(Action<string> onSuccessAction, Action<string> onFailedAction) =>
            Call(TestApi, onSuccessAction, onFailedAction);

        // CreateDeliveryMan
        public static void CreateRider(Action<string> onSuccessAction, Action<string> onFailedAction) =>
            Call(CreateRiderApi, onSuccessAction, onFailedAction);

        // CreateDeliveryOrder
        public static void CreateDeliveryOrder(DeliveryOrderDto orderDto, Action<string> onSuccessAction,
            Action<string> onFailedAction) => Call(CreateDeliveryOrderApi, JsonConvert.SerializeObject(orderDto),
            onSuccessAction, onFailedAction);

        // AssignDeliveryMan
        public static void AssignRider(DeliveryAssignmentDto assignmentDto, Action<string> onSuccessAction,
            Action<string> onFailedAction) => Call(AssignRiderApi, JsonConvert.SerializeObject(assignmentDto),
            onSuccessAction, onFailedAction);

        // UpdateOrderStatus
        public static void UpdateOrderStatus(DeliverySetStatusDto setStatusDto, Action<string> onSuccessAction,
            Action<string> onFailedAction) => Call(UpdateOrderStatusApi, JsonConvert.SerializeObject(setStatusDto),
            onSuccessAction, onFailedAction);
    }
}
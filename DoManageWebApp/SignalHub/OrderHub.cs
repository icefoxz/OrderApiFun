using Microsoft.AspNetCore.SignalR;
using OrderApiFun.Core.Services;
using System.Security.Claims;
using Azure.Identity;
using OrderDbLib;
using OrderHelperLib.Contracts;
using Utls;
using Auth = WebUtlLib.Auth;

namespace DoManageWebApp.SignalHub
{
    public class OrderHub : Hub
    {
        public const string RiderGroup = "RiderGroup";
        public const string UserGroup = "UserGroup";
        private ILogger<OrderHub> Log { get; }
        private JwtTokenService JwtTokenService { get; }
        private SignalRCallService SignalRCallService { get; }

        public OrderHub(JwtTokenService jwtTokenService, ILogger<OrderHub> log, SignalRCallService signalRCallService)
        {
            Log = log;
            SignalRCallService = signalRCallService;
            JwtTokenService = jwtTokenService;
        }

        public async Task<string> SignalRCall(string message)
        {
            try
            {
                var bag = DataBag.Deserialize(message);
                var method = bag.DataName;
                var result = await SignalRCallService.Invoke(Context, method, bag);
                return result;
            }
            catch (Exception e)
            {
                Log.Log(LogLevel.Information, e.ToString());
                Abort();
                return "Unauthorized";
            }
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var principal = httpContext?.User;
            if (principal?.Identity is not { IsAuthenticated: true })
            {
                Abort();
                return;
            }

            if (principal.IsInRole(Auth.Role_User))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup);
            }
            else if (principal.IsInRole(Auth.Role_Rider))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, RiderGroup);
            }
            else
            {
                Abort();
                return;
            }

            await base.OnConnectedAsync();
        }

        private void Abort()
        {
            Context.Abort(); // 如果令牌无效，则终止连接
        }
    }

    /// <summary>
    /// SignalR客户端请求服务
    /// </summary>
    public class SignalRCallService
    {
        private enum Roles
        {
            User,
            Rider
        }

        private IHubContext<OrderHub> _hub;
        private OrderDbContext _db;

        public SignalRCallService(OrderDbContext db, IHubContext<OrderHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public Task<string> Invoke(HubCallerContext context, string method, DataBag bag)
        {
            return method switch
            {
                SignalREvents.Req_Do_Vers =>Req_Do_Vers(context,bag),
                _ => TestMethod(context, method),
            };
        }

        private async Task<string> Req_Do_Vers(HubCallerContext context, DataBag bag)
        {
            var ids = bag.Get<List<long>>(0);
            var dic = await _db.DeliveryOrders.GetVersionsAsync(ids);
            return DataBag.SerializeWithName(SignalREvents.Req_Do_Vers, dic);
        }

        private Task<string> TestMethod(HubCallerContext context, string method)
        {
            var (userId, role) = GetInfo(context);
            if(role == Roles.User) return Task.FromResult($"User({userId}).{method}: Invoke!");
            return Task.FromResult($"Rider({GetRiderId(context)}).{method}: Invoke!");
        }

        private static (string userId, Roles role) GetInfo(HubCallerContext context)
        {
            var roleText = context.User.FindFirstValue(ClaimTypes.Role);
            var userId = context.UserIdentifier;
            var role = roleText switch
            {
                Auth.Role_User => Roles.User,
                Auth.Role_Rider => Roles.Rider,
                _ => throw new AuthenticationFailedException($"Unknown role: {roleText}")
            };
            return (userId, role);
        }

        private static long GetRiderId(HubCallerContext context)
        {
            var riderId = context.User.FindFirstValue(Auth.RiderId);
            return riderId == null
                ? throw new AuthenticationFailedException($"RiderId not found in token: {context.User}")
                : long.Parse(riderId);
        }
    }

    /// <summary>
    /// 服务器调用客户端服务
    /// </summary>
    public class ServerCallService
    {
        private IHubContext<OrderHub> Hub { get; }
        public ServerCallService(IHubContext<OrderHub> hub)
        {
            Hub = hub;
        }

        public async Task CallUserAsync(string userId, string method, params object[] args)
        {
            var user = Hub.Clients.User(userId);
            await user.SendCoreAsync(method, args);
            //var bag = DataBag.Deserialize(arg);
        }

        public async Task BroadcastRidersAsync(string method, params object[] args)
        {
            var group = Hub.Clients.Group(OrderHub.RiderGroup);
            await group.SendCoreAsync(method, args);
        }

        public async Task BroadCastAllAsync(string method, params object[] args)
        {
            await Hub.Clients.All.SendCoreAsync(method, args);
        }

        public async Task BroadCastUsersAsync(string method, params object[] args)
        {
            await Hub.Clients.Group(OrderHub.UserGroup).SendCoreAsync(method, args);
        }
    }
}

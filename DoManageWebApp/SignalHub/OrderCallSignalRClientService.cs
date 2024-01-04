using DoManageWebApp.Controllers.Api;
using OrderDbLib;
using OrderHelperLib.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using OrderHelperLib;
using WebUtlLib;
using Utls;

namespace DoManageWebApp.SignalHub;

//处理订单的SignalR更新服务, 根据不同的状态向不同的客户端推送消息
public class OrderCallSignalRClientService
{
    private ServerCallService _serverCall;
    private ILoggerFactory _factory;
    private OrderDbContext _db;

    public OrderCallSignalRClientService(ServerCallService serverCall, ILoggerFactory factory, OrderDbContext db)
    {
        _serverCall = serverCall;
        _factory = factory;
        _db = db;
    }

    public async Task CallOrderAsync(long orderId)
    {
        var log = GetLog();
        var o = await _db.DeliveryOrders
            .AsNoTracking()
            .Include(o => o.Rider)
            .Select(o => new
            {
                Sender = o.UserId,
                Receiver = o.ReceiverInfo.UserId,
                o.SubState,
                o.Status,
                o.Rider,
                o.Version,
                o.IsDeleted,
                o.Id
            })
            .FirstOrDefaultAsync(o => !o.IsDeleted && o.Id == orderId);

        if (o == null) return;
        var bag = DataBag.SerializeWithName(SignalREvents.Call_Do_Ver, o.Id, o.Version, o.Status, o.SubState);

        OnSenderUpdate();
        OnReceiverUpdate();
        OnRiderUpdate();
        return;

        void OnRiderUpdate()
        {
            //update to rider(s)
            if (!string.IsNullOrEmpty(o.Rider?.UserId))
            {
                CallUser(o.Rider.UserId, bag);
                log.Event($"UpdateOrder({orderId}) to Rider({o.Rider.UserId} : {bag})");
            }
            
            BroadcastRiders(bag);
            log.Event($"UpdateOrder({orderId}) to Riders({bag})");
            return;

            //bool ShouldBroadcastToRider()
            //{
            //    var isCreatedStatus = o.Status.ConvertToDoStatus() == DeliveryOrderStatus.Created;
            //    var isCreatedSubState = DoStateMap.GetState(o.SubState)?.GetStatus == DeliveryOrderStatus.Created;
            //
            //}
        }

        void OnReceiverUpdate()
        {
            //update to receiver
            if (!string.IsNullOrEmpty(o.Receiver))
            {
                CallUser(o.Receiver, bag);
                log.Event($"UpdateOrder({orderId}) to Receiver({o.Receiver} : {bag})");
            }
        }

        void OnSenderUpdate()
        {
            //update to sender
            CallUser(o.Sender, bag);
            log.Event($"UpdateOrder({orderId}) to Sender({o.Sender} : {bag})");
        }
    }

    private async void BroadcastRiders(string bag) =>
        await _serverCall.BroadcastRidersAsync(SignalREvents.ServerCall, bag);

    public async void CallUser(string userId, string bag) => 
        await _serverCall.CallUserAsync(userId, SignalREvents.ServerCall, bag);

    private ILogger GetLog([CallerMemberName] string? method = null)
    {
        var log = _factory.CreateLogger<ServerCallController>();
        log.Event(method);
        return log;
    }
}
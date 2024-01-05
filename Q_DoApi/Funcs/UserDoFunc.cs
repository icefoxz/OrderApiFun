using System.Net;
using Mapster;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Lingaus;
using Q_DoApi.Core.Extensions;
using Q_DoApi.Core.Services;
using Utls;
using WebUtlLib;

namespace Q_DoApi.Funcs;

public class UserDoFunc
{
    private LingauManager LingauManager { get; }
    private DoService DoService { get; }
    private SignalRCall SignalRCall { get; }
    public UserDoFunc(DoService doService, 
        LingauManager lingauManager, 
        SignalRCall signalRCall)
    {
        DoService = doService;
        LingauManager = lingauManager;
        SignalRCall = signalRCall;
    }

    [Function(nameof(User_Get_Active))]
    public async Task<HttpResponseData> User_Get_Active(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var f = await req.GetBagWithLogAsync(context);
        var (funcName, bag, log) = f;
        string? userId;
        int pageSize;
        int pageIndex;
        try
        {
            userId = context.GetUserId();
            pageSize = bag.Get<int>(0);
            pageIndex = bag.Get<int>(1);
            var doPg = await GetActivePageList(userId, f, pageIndex, pageSize);
            return await req.WriteBagAsync(funcName, doPg);
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }
    }

    [Function(nameof(User_Get_Histories))]
    public async Task<HttpResponseData> User_Get_Histories(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var f = await req.GetBagWithLogAsync(context);
        var (funcName, bag, log) = f;
        string? userId;
        int pageSize;
        int pageIndex;
        try
        {
            userId = context.GetUserId();
            pageSize = bag.Get<int>(0);
            pageIndex = bag.Get<int>(1);
            var doPg = await GetHistoryPageList(userId, f, pageIndex, pageSize);
            return await req.WriteBagAsync(funcName, doPg);
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }
    }

    [Function(nameof(User_Get_SubStates))]
    public async Task<HttpResponseData> User_Get_SubStates(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        return await req.WriteBagAsync(funcName, new object[] { DoStateMap.GetAllSubStates().ToArray() });
    }

    [Function(nameof(User_Do_Create))]
    public async Task<HttpResponseData> User_Do_Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
        var userId = context.GetUserId();
        // Deserialize the request body to DeliveryOrder
        DeliverOrderModel? orderDto = null;
        try
        {
            orderDto = bag.Get<DeliverOrderModel>(0);
        }
        catch (Exception e)
        {
            log.Event($"Invalid request body.\n{e}");
            return await req.WriteStringAsync("Invalid request body.");
        }
        var result = await DoService.Do_CreateAsync(userId, orderDto, log);
        if (!result.IsSuccess) return await req.WriteStringAsync(result.Message);
        SignalRCall.Update_Do_Call(result.Data.Id, log);
        return await req.WriteBagAsync(funcName, result.Data.Adapt<DeliverOrderModel>());
    }

    [Function(nameof(User_DoPay_Credit))]
    public async Task<HttpResponseData> User_DoPay_Credit(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        // Retrieve the request body
        var userId = context.GetUserId();
        DeliveryOrder? order;
        try
        {
            var deliveryOrderId = bag.Get<long>(0);
            var result = await DoService.DoPay_ByLingau(userId, deliveryOrderId, log);
            if (!result.IsSuccess)
                return await req.WriteStringAsync(result.Message);

            SignalRCall.Update_Do_Call(deliveryOrderId, log);
            return await req.WriteBagAsync(funcName, result.Data.Adapt<LingauModel>());
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }
    }    
    
    [Function(nameof(User_Do_GetPrice))]
    public async Task<HttpResponseData> User_Do_GetPrice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        // Retrieve the request body
        try
        {
            var order = bag.Get<DeliverOrderModel>(0);
            var result = await DoService.Do_GetPrice(order, log);
            if (!result.IsSuccess)
                return await req.WriteStringAsync(result.Message);
            var dto = result.Data;
            return await req.WriteBagAsync(funcName, dto);
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }
    }    
    
    [Function(nameof(User_DoPay_Rider))]
    public async Task<HttpResponseData> User_DoPay_Rider(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        // Retrieve the request body
        var userId = context.GetUserId();
        DeliveryOrder? order;
        try
        {
            var deliveryOrderId = bag.Get<long>(0);
            var result = await DoService.DoPay_RiderCollect(userId, deliveryOrderId, log);
            if (!result.IsSuccess)
                return await req.WriteStringAsync(result.Message);

            SignalRCall.Update_Do_Call(deliveryOrderId, log);
            return await req.WriteBagAsync(funcName, result.Data.Adapt<DeliverOrderModel>());
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }
    }

    [Function(nameof(User_Do_StatusUpdate))]
    public async Task<HttpResponseData> User_Do_StatusUpdate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (functionName,bag,log) = await req.GetBagWithLogAsync(context);
        var orderId = bag.Get<long>(0);
        var subState = bag.Get<string>(1);
        var result =
            await UseDo_StateUpdate(orderId, context.GetUserId(), subState, log);
        if (result.IsSuccess)
            return await req.WriteBagAsync(functionName, TypeAdapter.Adapt<DeliverOrderModel>(result.Data));

        SignalRCall.Update_Do_Call(orderId, log);
        return await req.WriteStringAsync(result.Message);

    }

    [Function(nameof(User_Do_Cancel))]
    public async Task<HttpResponseData> User_Do_Cancel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        var (functionName, b, log) = await req.GetBagWithLogAsync(context);
        var orderId = b.Get<long>(0);
        var result = await UseDo_StateUpdate(orderId, userId, DoSubState.SenderCancelState, log);
        if (!result.IsSuccess) return await req.WriteStringAsync(result.Message);

        SignalRCall.Update_Do_Call(orderId, log);
        return await req.WriteBagAsync(functionName, result.Data.Adapt<DeliverOrderModel>());
    }

    private async Task<PageList<DeliverOrderModel>> GetHistoryPageList(string userId, (string functionName, DataBag bag, ILogger log) f, int pageIndex = 0, int pageSize = 20)
    {
        var historyPl = await DoService.User_DoPage_GetAsync(userId, pageSize, pageIndex, d => d.Status < 0, f.log);
        var hpl = historyPl.AdaptPageList<DeliveryOrder, DeliverOrderModel>(f.log);
        return hpl;
    }

    private async Task<PageList<DeliverOrderModel>> GetActivePageList(string userId, (string functionName, DataBag bag, ILogger log) f,int pageIndex = 0,int pageSize = 50)
    {
        var orderPl = await DoService.User_DoPage_GetAsync(userId, pageSize, pageIndex, d => d.Status >= 0, f.log);
        var opl = orderPl.AdaptPageList<DeliveryOrder, DeliverOrderModel>(f.log);
        return opl;
    }

    /// <summary>
    /// 更新订单状态 
    /// </summary>
    private async Task<ResultOf<DeliveryOrder>> UseDo_StateUpdate(long orderId, string userId, string subState,
        ILogger log)
    {
        var order = await UserDo_Get(userId, orderId);
        if (order == null)
            return ResultOf.Fail<DeliveryOrder>("Order not found.");
        return await DoService.Do_SubState_Update(order, TransitionRoles.User, subState, log);
    }

    private async Task<DeliveryOrder?> UserDo_Get(string userId, long deliveryOrderId)
    {
        var order = await DoService.Do_FirstAsync(o => o.UserId == userId && o.Id == deliveryOrderId);
        return order;
    }
}
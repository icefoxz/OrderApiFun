using System.Net;
using Mapster;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Lingaus;
using OrderHelperLib.Req_Models.Users;
using Q_DoApi.Core.Extensions;
using Q_DoApi.DtoMapping;
using Utls;
using WebUtlLib;

namespace Do_Api.Funcs;

public class UserDoFunc
{
    private LingauManager LingauManager { get; }
    private DoService DoService { get; }
    public UserDoFunc(DoService doService, LingauManager lingauManager)
    {
        DoService = doService;
        LingauManager = lingauManager;
    }

    [Function(nameof(User_Get_Active))]
    public async Task<HttpResponseData> User_Get_Active(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
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
        [HttpTrigger(AuthorizationLevel.Function, "post")]
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
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        return await req.WriteBagAsync(funcName, new object[] { DoStateMap.GetAllSubStates().ToArray() });
    }

    [Function(nameof(User_Do_Create))]
    public async Task<HttpResponseData> User_Do_Create(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(User_Do_Create));
        var bag = await req.GetBagAsync();
        //test Instance:
        //var dto = InstanceTestDeliverDto();
        //log.LogWarning(Json.Serialize(dto));
        //throw new NotImplementedException();
        log.LogInformation("C# HTTP trigger function processed a request.");
        var userId = context.Items[Auth.UserId].ToString();
        // Deserialize the request body to DeliveryOrder
        DeliverOrderModel? orderDto = null;
        try
        {
            orderDto = bag.Get<DeliverOrderModel>(0);
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid request body.");
            return badRequestResponse;
        }

        if (!MyPhone.VerifyPhoneNumber(orderDto.SenderInfo.PhoneNumber))
        {
            log.LogWarning("Invalid sender phone number.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid sender phone number.");
            return badRequestResponse;
        }

        if (!MyPhone.VerifyPhoneNumber(orderDto.ReceiverInfo.PhoneNumber))
        {
            log.LogWarning("Invalid receiver phone number.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid receiver phone number.");
            return badRequestResponse;
        }

        // Add the new order to the database using the DeliveryOrderService
        var newDo = await DoService.CreateDeliveryOrderAsync(userId, orderDto, log);
        var createdResponse = req.CreateResponse(HttpStatusCode.Created);
        //createdResponse.Headers.Add("Location", $"deliveryorder/{newOrder.Id}");
        await createdResponse.WriteStringAsync(DataBag.Serialize(newDo.Adapt<DeliverOrderModel>()));
        return createdResponse;
    }

    [Function(nameof(User_Do_PayByCredit))]
    public async Task<HttpResponseData> User_Do_PayByCredit(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (funcName,bag,log) = await req.GetBagWithLogAsync(context);
        // Retrieve the request body
        var userId = context.GetUserId();
        DeliveryOrder? order;
        try
        {
            var deliveryOrderId = bag.Get<int>(0);
            order = await UserDo_Get(userId, deliveryOrderId);
        }
        catch (Exception e)
        {
            log.LogWarning($"Invalid request body.\n{e}");
            return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
        }

        var userLingau = await LingauManager.GetLingauAsync(userId);
        if (order.PaymentInfo.Charge > userLingau.Credit)
            return await req.WriteStringAsync("Insufficient Lingau.");

        await DoService.PayDeliveryOrderByLingauAsync(userId, order, log);
        return await req.WriteStringAsync(DataBag.Serialize(userLingau.Adapt<LingauModel>()));
    }

    [Function(nameof(User_Do_StatusUpdate))]
    public async Task<HttpResponseData> User_Do_StatusUpdate(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var (functionName,bag,log) = await req.GetBagWithLogAsync(context);
        var orderId = bag.Get<long>(0);
        var subState = bag.Get<int>(1);
        var result =
            await UseDo_StateUpdate(orderId, context.GetUserId(), subState, log);
        if (result.IsSuccess)
            return await req.WriteStringAsync(DataBag.Serialize(TypeAdapter.Adapt<DeliverOrderModel>(result.Data)));
        return await req.WriteStringAsync(result.Message);

    }

    [Function(nameof(User_Do_Cancel))]
    public async Task<HttpResponseData> User_Do_Cancel(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        var f = await req.GetBagWithLogAsync(context);
        var orderId = f.bag.Get<long>(0);
        var result = await UseDo_StateUpdate(orderId, userId, DoSubState.SenderCancelState, f.log);
        if (!result.IsSuccess) return await req.WriteStringAsync(result.Message);
        return await ResponseDoData(req, userId, f);
    }

    //返回Order和History更新
    private async Task<HttpResponseData> ResponseDoData(HttpRequestData req, string userId, (string functionName, DataBag bag, ILogger log) f)
    {
        var opl = await GetActivePageList(userId, f);
        var hpl = await GetHistoryPageList(userId, f);
        return await req.WriteStringAsync(DataBag.SerializeWithName(f.functionName, opl, hpl));
    }

    private async Task<PageList<DeliverOrderModel>> GetHistoryPageList(string userId, (string functionName, DataBag bag, ILogger log) f, int pageIndex = 0, int pageSize = 20)
    {
        var historyPl = await DoService.User_GetDeliveryOrdersAsync(userId, pageSize, pageIndex, d => d.Status < 0, f.log);
        var hpl = historyPl.AdaptPageList<DeliveryOrder, DeliverOrderModel>();
        return hpl;
    }

    private async Task<PageList<DeliverOrderModel>> GetActivePageList(string userId, (string functionName, DataBag bag, ILogger log) f,int pageIndex = 0,int pageSize = 50)
    {
        var orderPl = await DoService.User_GetDeliveryOrdersAsync(userId, pageSize, pageIndex, d => d.Status >= 0, f.log);
        var opl = orderPl.AdaptPageList<DeliveryOrder, DeliverOrderModel>();
        return opl;
    }

    /// <summary>
    /// 更新订单状态 bag[doId,subState]
    /// </summary>
    private async Task<ResultOf<DeliveryOrder>> UseDo_StateUpdate(long orderId, string userId,int subState, ILogger log)
    {
        var order = await UserDo_Get(userId, orderId);
        if (order == null)
            return ResultOf.Fail<DeliveryOrder>("Order not found.");
        return await DoService.SubState_Update(order, TransitionRoles.User, subState, log);
    }

    private async Task<DeliveryOrder?> UserDo_Get(string userId, long deliveryOrderId)
    {
        var order = await DoService.GetFirstAsync(o => o.UserId == userId && o.Id == deliveryOrderId);
        return order;
    }
}
using System.Net;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Dtos.Users;
using Q_DoApi.Core.Extensions;
using Q_DoApi.Core.Services;
using Utls;

namespace Do_Api.Funcs;

public class RiderFunc
{
    private LingauManager LingauManager { get; }
    private RiderManager RiderManager { get; }
    private UserManager<User> UserManager { get; }

    public RiderFunc(RiderManager riderManager, UserManager<User> userManager, LingauManager lingauManager)
    {
        RiderManager = riderManager;
        UserManager = userManager;
        LingauManager = lingauManager;
    }

    //创建rider
    [Function(nameof(User_TransformToRider))]
    public async Task<HttpResponseData> User_TransformToRider([HttpTrigger(AuthorizationLevel.Function, "post")] 
        HttpRequestData req, 
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(User_TransformToRider));
        // 解析请求主体获取用户ID
        var userId = context.Items[Auth.UserId].ToString();
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) throw new NullReferenceException($"User not found! Id = {userId}");
        // 调用DeliveryManManager创建DeliveryMan
        var deliveryMan = await RiderManager.CreateRiderAsync(user);

        // 返回响应
        var response = req.CreateResponse(HttpStatusCode.Created);
        // databag 序列化为json字符串
        await response.WriteStringAsync(DataBag.Serialize($"DeliveryMan created with ID: {deliveryMan.Id}"));
        return response;
    }

    [Function(nameof(Rider_Create))]
    public async Task<HttpResponseData> Rider_Create(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context)
    {
        var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
        // 解析请求主体获取用户ID
        var riderId = context.GetRiderId();
        var rider = await RiderManager.FindByIdAsync(riderId);
        var userId = rider.UserId;
        var transferAmount = bag.Get<float>(0);
        var transferTargetUserId = bag.Get<string>(1);
        var result = await LingauManager.TransferLingauAsync(userId, transferTargetUserId, transferAmount, log);
        if (!result.IsSuccess)
            return await req.WriteStringAsync(result.Message);
        return await req.WriteBagAsync(funcName, result.Data.Adapt<UserModel>());
    }

    //rider转账给user
    [Function(nameof(Rider_TransferToUser))]
    public async Task<HttpResponseData> Rider_TransferToUser(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context)
    {
        var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
        // 解析请求主体获取用户ID
        var riderId = context.GetRiderId();
        var rider = await RiderManager.FindByIdAsync(riderId);
        var userId = rider.UserId;
        var transferAmount = bag.Get<float>(0);
        var transferTargetUserId = bag.Get<string>(1);
        var result = await LingauManager.TransferLingauAsync(userId, transferTargetUserId, transferAmount, log);
        if (!result.IsSuccess)
            return await req.WriteStringAsync(result.Message);
        return await req.WriteBagAsync(funcName, result.Data.Adapt<UserModel>());
    }
}
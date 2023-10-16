using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
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

    //rider转账给user
    [Function(nameof(Rider_TransferToUser))]
    public async Task<HttpResponseData> Rider_TransferToUser(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(Rider_TransferToUser));
        // 解析请求主体获取用户ID
        int.TryParse(context.Items[Auth.RiderId].ToString(), out var riderId);
        var rider = await RiderManager.FindByRiderIdAsync(riderId);
        if (rider == null) throw new NullReferenceException($"Rider not found! Id = {riderId}");
        var userId = context.Items[Auth.UserId].ToString();
        var bag = await req.GetBagAsync();
        var transferAmount = bag.Get<float>(0);
        var transferTargetUserId = bag.Get<string>(1);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await LingauManager.TransferLingauAsync(userId, transferTargetUserId, transferAmount, log);
        return response;
    }
}
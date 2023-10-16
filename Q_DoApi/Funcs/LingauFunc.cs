using System.Net;
using Mapster;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderHelperLib;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Lingaus;
using Utls;

namespace Do_Api.Funcs;

public class LingauFunc
{
    private LingauManager LingauManager { get; }

    public LingauFunc(LingauManager lingauManager)
    {
        LingauManager = lingauManager;
    }

    [Function(nameof(User_GetLingau))]
    public async Task<HttpResponseData> User_GetLingau(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(User_GetLingau));
        log.LogInformation("C# HTTP trigger function processed a request.");

        var userId = context.Items[Auth.UserId].ToString();
        var bag = await req.GetBagAsync();
        // Retrieve the user's Lingau balance
        var lingau = await LingauManager.GetLingauAsync(userId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(DataBag.Serialize(lingau.Adapt<LingauModel>()));
        return response;
    }
}
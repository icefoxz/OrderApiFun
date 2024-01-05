using System.Net;
using System.Security.Claims;
using Mapster;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Services;
using OrderApiFun.Core.Tokens;
using OrderDbLib.Entities;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using Q_DoApi.Core.Extensions;
using Q_DoApi.Core.Services;
using Utls;
using WebUtlLib;

namespace Q_DoApi.Funcs;

public class ImageFunc
{
    private const string InvalidToken = "Invalid token.";
    private BlobService _blobService { get; }
    private DoService _doService { get; }
    private JwtTokenService _jwtTokenService { get; }
    public ImageFunc(BlobService blobService, JwtTokenService jwtTokenService, DoService doService)
    {
        _blobService = blobService;
        _jwtTokenService = jwtTokenService;
        _doService = doService;
    }

    [Function(nameof(Anonymous_UploadImage))]
    public async Task<HttpResponseData> Anonymous_UploadImage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger<ImageFunc>();
        log.Event();
        var fileName = await UploadImageAsync(req.Body, log);
        return await req.WriteStringAsync(fileName);
    }

    [Function(nameof(Rider_Image_Do))]
    public async Task<HttpResponseData> Rider_Image_Do(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger<ImageFunc>();
        log.Event();
        var token = req.Query["token"];
        if (string.IsNullOrWhiteSpace(token)) return await req.WriteStringAsync("Invalid request.");
        var tokenResult = await _jwtTokenService.ValidateTokenAsync(token);
        switch (tokenResult.Result)
        {
            case TokenValidation.Results.Expired: return await req.WriteStringAsync("Token expired.");
            case TokenValidation.Results.Error: return await req.WriteStringAsync(InvalidToken);
            case TokenValidation.Results.Valid: break;
            default: throw new ArgumentOutOfRangeException();
        }
        var principal = tokenResult.Principal;
        var riderId = context.GetRiderId();
        var userId = context.GetUserId();
        if (!principal.HasClaim(Auth.RiderId, riderId.ToString())) return await req.WriteStringAsync(InvalidToken);
        if (!principal.HasClaim(ClaimTypes.NameIdentifier, userId)) return await req.WriteStringAsync(InvalidToken);
        var oid = principal.FindFirst(JwtTokenService.OrderId)?.Value;
        var subState = principal.FindFirst(JwtTokenService.OrderSubState)?.Value;
        var oStatus = principal.FindFirst(JwtTokenService.OrderStatus)?.Value;
        var segmentIndex = principal.FindFirst(JwtTokenService.OrderSegmentIndex)?.Value;
        if (string.IsNullOrWhiteSpace(oid) || !long.TryParse(oid, out var orderId))
            return await req.WriteStringAsync(InvalidToken);
        if(string.IsNullOrWhiteSpace(subState)) return await req.WriteStringAsync(InvalidToken);
        if (string.IsNullOrWhiteSpace(segmentIndex) || !int.TryParse(segmentIndex, out var segIndex))
            return await req.WriteStringAsync(InvalidToken);
        if (string.IsNullOrWhiteSpace(oStatus) || !int.TryParse(oStatus, out var orderStatus)) return await req.WriteStringAsync(InvalidToken);
        var order = await _doService.Do_FirstAsync(o =>
            o.Id == orderId &&
            o.RiderId == riderId);
        if(order==null)return await req.WriteStringAsync("Order not found!");
        var isOnState = order.SubState == subState && order.Status == orderStatus && ((DeliveryOrderStatus)order.Status).IsInProgress();
        if(!isOnState)return await req.WriteStringAsync("Order state not matched!");
        var imageGuid = await UploadImageAsync(req.Body, log);
        if (string.IsNullOrWhiteSpace(imageGuid)) return await req.WriteStringAsync("Upload image failed.");
        var imgResult = await _doService.Do_Images_Add(orderId, subState, segIndex, imageGuid, log);
        if (imgResult.IsSuccess)
            return await req.WriteBagAsync(nameof(Rider_Image_Do), imgResult.Data.Adapt<DeliverOrderModel>());
        return await req.WriteStringAsync(imgResult.Message);
    }

    private async Task<string?> UploadImageAsync(Stream stream, ILogger log)
    {
        //读取文件数据和文件类型（例如：jpg, png, gif）

        //传递文件数据和文件类型到BlobService进行上传
        string fileId;
        try
        {
            fileId = await _blobService.UploadFileAsync(stream, log);
        }
        catch (ArgumentException e)
        {
            log.Event($"Failed: {e.Message}");
            return null;
        }

        //创建响应，返回上传文件的Id
        return fileId;
    }
}
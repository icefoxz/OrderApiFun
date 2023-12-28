using Microsoft.Extensions.Logging;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using Q_DoApi.Core.Utls;

namespace Q_DoApi.Core.Services;

public static class SignalRCall
{
    private static string _serverUrl;

    public static string ServerUrl
    {
        get
        {
            if(_serverUrl == null)
                throw new NullReferenceException("SignalRCallService.ServerUrl is null!");
            return _serverUrl;
        }
    }

    public static void Init(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public static void Update_Do_Call(long orderId, ILogger log) =>
        Task.Run(async () => await ApiCaller.ApiPost(_serverUrl, SignalREvents.ServerCall, SignalREvents.Call_Do_Ver,
            DataBag.SerializeWithName(SignalREvents.Call_Do_Ver, orderId), log));
}
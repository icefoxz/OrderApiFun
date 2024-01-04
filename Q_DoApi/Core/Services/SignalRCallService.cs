using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using Q_DoApi.Core.Utls;
using Utls;

namespace Q_DoApi.Core.Services;

public class SignalRCall
{
    private readonly string _serverUrl = Config.GetSignalRServerUrl();

    public string ServerUrl
    {
        get
        {
            if(_serverUrl == null)
                throw new NullReferenceException("SignalRCallService.ServerUrl is null!");
            return _serverUrl;
        }
    }

    public void Update_Do_Call(long orderId, ILogger log)
    {
        Task.Run(() => ApiCaller.ApiPost(ServerUrl,
            SignalREvents.ServerCall,
            SignalREvents.Call_Do_Ver,
            DataBag.SerializeWithName(SignalREvents.Call_Do_Ver, orderId), log));
    }
}